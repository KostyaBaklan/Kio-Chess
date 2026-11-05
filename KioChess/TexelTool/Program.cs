using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Pgn.Models;
using Engine.Pgn.Streaming;
using Engine.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using TexelTool.Configuration;
using TexelTool.Models;

namespace TexelTool
{
    internal static partial class Program
    {
        private static TexelConfiguration _cfg;
        private static IConfigurationProvider _configuration;
        private static IStaticValueProvider _staticValueProvider;
        private static StaticTableCollection _collection;
        private static IEvaluationServiceFactory _factory;
        private static MoveHistoryService _history;
        private static Board _board;
        private static DataAccess.Interfaces.IAppDbService _appDbService;

        private static readonly string[] PhaseNames = ["Middle", "End"];

        private static void Main(string[] args)
        {
            bool worker = args.Length >= 4 && args[0] == "--worker";

            StreamWriter log = null;

            try
            {
                _cfg = JsonConvert.DeserializeObject<TexelConfiguration>(File.ReadAllText(@"Config\TexelConfiguration.json"));
                TexelDataSet.ValidationEvery = _cfg.ValidationEvery;

                if (!worker)
                {
                    Directory.CreateDirectory(_cfg.OutputDirectory);
                    var logPath = Path.Combine(_cfg.OutputDirectory, $"texel_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    log = new StreamWriter(logPath, false) { AutoFlush = true };
                    Console.SetOut(TextWriter.Synchronized(new TeeWriter(Console.Out, log)));
                    Console.WriteLine($"Log file: {Path.GetFullPath(logPath)}");
                }

                Boot.SetUp();

                _appDbService = Boot.GetService<DataAccess.Interfaces.IAppDbService>();
                _appDbService.Connect();

                _configuration = Boot.GetService<IConfigurationProvider>();
                _staticValueProvider = Boot.GetService<IStaticValueProvider>();
                _collection = ((StaticValueProvider)_staticValueProvider).Collection;
                _factory = Boot.GetService<IEvaluationServiceFactory>();
                _history = Boot.GetService<MoveHistoryService>();

                var position = new Position();
                _board = position.GetBoard();

                if (worker)
                {
                    RunWorker(int.Parse(args[1]), int.Parse(args[2]), args[3], args.Length >= 5 && args[4] == "1");
                    return;
                }

                TexelDataSet data;
                double k;

                if ((_cfg.ReuseDataset || File.Exists(CheckpointPath)) && File.Exists(_cfg.DatasetFile))
                {
                    data = TexelDataSet.Load(_cfg.DatasetFile, out k);
                    Console.WriteLine($"Loaded {data.Count} positions from {_cfg.DatasetFile}, K = {k:F4}");
                }
                else
                {
                    data = Extract(position);
                    if (File.Exists(UsagePath)) File.Delete(UsagePath);
                    data.Freeze();

                    if (_cfg.DeletePgnAfterExtraction)
                    {
                        foreach (var f in PgnFiles.Enumerate(_cfg.PgnPath)) File.Delete(f);
                        Console.WriteLine($"Deleted PGN file(s) at {_cfg.PgnPath}");
                    }
                    Console.WriteLine($"Extracted {data.Count} positions");

                    FilterByScore(data);
                    Console.WriteLine($"After score filter: {data.Count} (Middle {data.PhaseIndices[0].Length}, End {data.PhaseIndices[1].Length})");

                    var baseScores = ScoreAll(data);
                    k = FitK(data, baseScores);
                    Console.WriteLine($"K = {k:F8}, error = {TotalError(data, baseScores, k):F8}");

                    data.Save(_cfg.DatasetFile, k);
                    Console.WriteLine($"Dataset saved to {Path.GetFullPath(_cfg.DatasetFile)}");

                    data = TexelDataSet.Load(_cfg.DatasetFile, out k);
                    GC.Collect();
                }

                var parameters = BuildParameters();
                if (!File.Exists(UsagePath)) ProbeUsage(data, parameters);
                ApplyUsage(parameters);
                Console.WriteLine($"Parameters: {parameters.Count}");
                ReportPieceRules("start");
                TryResume(parameters);

                VerifyIncremental(data, parameters, k);

                if (_cfg.WorkerCount > 1)
                {
                    TuneParallel(parameters, Path.GetFullPath(_cfg.DatasetFile), data, k);
                }
                else
                {
                    Tune(data, parameters, k);
                }

                if (_cfg.RecentreTables && _cfg.TunePieceValues)
                {
                    var before = ScoreAll(data);
                    Recentre();
                    var after = ScoreAll(data);
                    int changed = 0;
                    for (int i = 0; i < before.Length; i++) if (before[i] != after[i]) changed++;
                    Console.WriteLine($"Recentre: {changed} of {before.Length} scores changed (expected 0)");
                }

                ReportPieceRules("final");
                var scores = ScoreAll(data);
                Console.WriteLine($"Final error = {TotalError(data, scores, k):F8}, validation = {ValidationError(data, k):F8}");

                Save(parameters);

                if (_cfg.DeleteDatasetOnSuccess)
                {
                    data.Dispose();
                    File.Delete(CheckpointPath);
                    File.Delete(_cfg.DatasetFile);
                    Console.WriteLine("Tuning finished: dataset and checkpoint deleted");
                }
                else File.Delete(CheckpointPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _appDbService?.Disconnect();
            }

            if (!worker) Console.WriteLine("Finished. Press Enter to exit.");
            log?.Flush();
            if (!worker) Console.ReadLine();
            log?.Dispose();
        }

        #region Extraction

        private static TexelDataSet Extract(Position position)
        {
            var data = new TexelDataSet();
            int games = 0, failures = 0, read = 0;
            var watch = Stopwatch.StartNew();
            long lastReport = 0;

            var files = PgnFiles.Enumerate(_cfg.PgnPath);
            if (files.Length == 0) throw new FileNotFoundException("No PGN files found at " + _cfg.PgnPath);
            Console.WriteLine($"[Extract] {files.Length} PGN file(s) from {_cfg.PgnPath}");

            foreach (var game in ReadAllGames(files))
            {
                if (games >= _cfg.MaxGames || data.Count >= _cfg.MaxPositions) break;

                read++;
                if (watch.ElapsedMilliseconds - lastReport >= _cfg.ProgressIntervalSeconds * 1000L)
                {
                    lastReport = watch.ElapsedMilliseconds;
                    Console.WriteLine($"[Extract] file {_fileIndex}/{files.Length} {_fileProgress:F1}% | read {read}, used {games}/{_cfg.MaxGames} games | positions {data.Count}/{_cfg.MaxPositions} | failures {failures} | {watch.Elapsed:hh\\:mm\\:ss}");
                }

                var result = ParseResult(game);
                if (result < 0) continue;
                if (!PassesEloAndTimeControl(game)) continue;
                if (game.Tags != null && (game.Tags.ContainsKey("FEN")
                    || (game.Tags.TryGetValue("SetUp", out var setUp) && setUp == "1"))) continue;

                games++;
                int made = 0;
                int skipPlies = new Random(games).Next(_cfg.SkipOpeningPlies, _cfg.SkipOpeningPliesMax + 1);
                try
                {
                    bool isWhite = true;
                    foreach (var move in game.Moves.Take(_cfg.MaxPliesPerGame))
                    {
                        if (move.Castling == CastlingType.None && string.IsNullOrEmpty(move.TargetSquare) && !string.IsNullOrEmpty(move.Notation) && move.Notation.Contains('.')) continue;
                        TexelPgnConverter.ProcessMove(position, move, isWhite);
                        made++;
                        isWhite = !isWhite;

                        if (made < skipPlies || (made - skipPlies) % _cfg.SampleEveryNthPosition != 0) continue;
                        if (_cfg.ExcludeCaptures && move.IsCapture) continue;
                        if (_cfg.ExcludeInCheck && (_board.IsCheckToWhite() || _board.IsCheckToBlack())) continue;

                        data.Add(_board, _history.GetPhase(), _history.GetCastleRights(), result);
                        if (data.Count >= _cfg.MaxPositions) break;
                    }
                }
                catch (Exception ex)
                {
                    failures++;
                    RecordFailure(ex, made);
                }
                finally
                {
                    for (int i = 0; i < made; i++) position.UnMake();
                }

                }

                Console.WriteLine($"[Extract] done: read {read}, used {games} games, positions {data.Count}, duplicates merged {data.Duplicates}, failures {failures}, {watch.Elapsed:hh\\:mm\\:ss}");
                PrintFailures();
                return data;
        }

        private static int _fileIndex;
        private static double _fileProgress;

        private static IEnumerable<PgnGame> ReadAllGames(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                _fileIndex = i + 1;
                Console.WriteLine($"[Extract] opening {Path.GetFileName(files[i])}");
                using var reader = new PgnStreamReader(files[i]);
                foreach (var game in reader.ReadGames())
                {
                    _fileProgress = reader.ProgressPercent;
                    yield return game;
                }
            }
        }

        private static readonly Dictionary<string, int> FailureCounts = new();
        private static readonly Dictionary<string, string> FailureExamples = new();
        private static long _failurePlySum;

        private static void RecordFailure(Exception ex, int plyReached)
        {
            var message = ex.Message ?? string.Empty;
            var key = ex.GetType().Name + ": " + (message.Length > 25 ? message.Substring(0, 25) : message);
            FailureCounts[key] = FailureCounts.GetValueOrDefault(key) + 1;
            if (!FailureExamples.ContainsKey(key)) FailureExamples[key] = message + " (after " + plyReached + " plies)";
            _failurePlySum += plyReached;
        }

        private static void PrintFailures()
        {
            int total = FailureCounts.Values.Sum();
            if (total == 0) return;

            Console.WriteLine("[Extract] failure breakdown (average ply reached " + (_failurePlySum / total) + "):");
            foreach (var pair in FailureCounts.OrderByDescending(p => p.Value))
            {
                Console.WriteLine("    " + pair.Value + " x " + pair.Key + " | e.g. " + FailureExamples[pair.Key]);
            }
        }

        private static float ParseResult(PgnGame game)
        {
            var text = game.ResultString;
            if ((string.IsNullOrEmpty(text)) && game.Tags != null) game.Tags.TryGetValue("Result", out text);
            return text?.Replace(" ", "") switch
            {
                "1-0" => 1f,
                "0-1" => 0f,
                "1/2-1/2" => 0.5f,
                _ => -1f
            };
        }

        // Lichess classifies by base + 40 * increment: below 180 s is bullet, below 480 s is blitz, below 1500 s is rapid.
        // Bullet is excluded when MinEstimatedGameSeconds > 0; a missing TimeControl counts as classical.
        private static bool PassesEloAndTimeControl(PgnGame game)
        {
            if (game.Tags == null) return false;
            if (!game.Tags.TryGetValue("WhiteElo", out var w) || !int.TryParse(w, out var white)) return false;
            if (!game.Tags.TryGetValue("BlackElo", out var b) || !int.TryParse(b, out var black)) return false;

            int estimated = int.MaxValue;
            if (game.Tags.TryGetValue("TimeControl", out var tc) && !string.IsNullOrEmpty(tc) && tc != "-")
            {
                var parts = tc.Split('+');
                if (int.TryParse(parts[0], out var baseSeconds))
                {
                    int increment = parts.Length > 1 && int.TryParse(parts[1], out var inc) ? inc : 0;
                    estimated = baseSeconds + 40 * increment;
                }
            }

            if (_cfg.MinEstimatedGameSeconds > 0 && estimated < _cfg.MinEstimatedGameSeconds) return false;

            int minElo = estimated < 180 ? _cfg.MinElo
                : estimated < 480 ? _cfg.MinEloBlitz
                : estimated < 1500 ? _cfg.MinEloRapid
                : _cfg.MinEloClassical;
            return Math.Min(white, black) >= minElo;
        }

        #endregion

        #region Evaluation / error

        private static int ScoreOne(TexelDataSet data, int index)
        {
            _board.SetPosition(data.PiecesOf(index));
            _history.SetEvaluationState(data.Phases[index], data.Castles[index]);
            return _board.EvaluateUncached();
        }

        private static int[] ScoreAll(TexelDataSet data)
        {
            _board.ResetEvaluationCaches();
            var scores = new int[data.Count];
            for (int i = 0; i < scores.Length; i++) scores[i] = ScoreOne(data, i);
            return scores;
        }

        private static void FilterByScore(TexelDataSet data)
        {
            var scores = ScoreAll(data);
            var keep = new bool[scores.Length];
            for (int i = 0; i < scores.Length; i++) keep[i] = Math.Abs(scores[i]) <= _cfg.MaxAbsStaticScore;
            data.Keep(keep);
        }

        private static double Sigmoid(int score, double k) => 1.0 / (1.0 + Math.Exp(-k * score / _cfg.SigmoidScale));

        private static double TotalError(TexelDataSet data, int[] scores, double k)
        {
            double sum = 0;
            var results = data.Results;
            for (int i = 0; i < scores.Length; i++)
            {
                double d = results[i] - Sigmoid(scores[i], k);
                sum += d * d;
            }
            return sum / scores.Length;
        }

        private static double FitK(TexelDataSet data, int[] scores)
        {
            const double phi = 0.6180339887498949;
            double a = _cfg.KMin, b = _cfg.KMax;
            double c = b - phi * (b - a), d = a + phi * (b - a);
            double fc = TotalError(data, scores, c), fd = TotalError(data, scores, d);

            for (int i = 0; i < _cfg.KFitIterations; i++)
            {
                if (fc < fd)
                {
                    b = d; d = c; fd = fc;
                    c = b - phi * (b - a);
                    fc = TotalError(data, scores, c);
                }
                else
                {
                    a = c; c = d; fc = fd;
                    d = a + phi * (b - a);
                    fd = TotalError(data, scores, d);
                }
            }
            double fitted = (a + b) / 2;
            if (fitted - _cfg.KMin < 0.01 || _cfg.KMax - fitted < 0.01)
                Console.WriteLine($"WARNING: K = {fitted:F4} is at the search bound [{_cfg.KMin}, {_cfg.KMax}]. Widen KMin/KMax.");
            return fitted;
        }

        // Sum of squared errors over positions of one phase (only that phase's parameters change).
        private static double PhaseError(TexelDataSet data, int phase, double k, int[] scoresOut = null)
        {
            var indices = data.PhaseIndices[phase];
            var pieces = data.Pieces;
            var phases = data.Phases;
            var castles = data.Castles;
            var results = data.Results;
            double sum = 0;
            for (int n = 0; n < indices.Length; n++)
            {
                int i = indices[n];
                _board.SetPosition(pieces.Slice(i * 12, 12));
                _history.SetEvaluationState(phases[i], castles[i]);
                int score = _board.EvaluateUncached();
                if (scoresOut != null) scoresOut[i] = score;
                double d = results[i] - Sigmoid(score, k);
                sum += d * d;
            }
            return sum;
        }

        private static double ValidationError(TexelDataSet data, double k)
        {
            if (data.ValidationCount == 0) return double.NaN;
            var results = data.Results;
            double sum = 0;
            for (int phase = 0; phase < 2; phase++)
            {
                foreach (int i in data.ValidationIndices[phase])
                {
                    double d = results[i] - Sigmoid(ScoreOne(data, i), k);
                    sum += d * d;
                }
            }
            return sum / data.ValidationCount;
        }

        private static void Apply(Parameter p, int value)
        {
            p.Set(value);
            var phases = p.PhaseSet;
            foreach (var phase in phases) _factory.GetEvaluationService(phase).Reload(_configuration, _staticValueProvider);
            if (p.Name.Contains("Shield")) _board.RebuildKingShieldLookup();
            foreach (var phase in phases) _board.ResetPawnHash(phase);
        }

        private static bool TryCandidate(Parameter p, int candidate, TexelDataSet data, double k, double[] phaseError)
        {
            if (p.IsValid != null && !p.IsValid(candidate)) return false;
            if (_useIncremental && p.LinearPiece >= 0) return TryLinear(p, candidate, data, k, phaseError);

            Apply(p, candidate);

            var phases = p.PhaseSet;
            double before = 0, after = 0;
            for (int j = 0; j < phases.Length; j++)
            {
                _trialErrors[j] = PhaseError(data, phases[j], k, _scratch);
                after += _trialErrors[j];
                before += phaseError[phases[j]];
            }
            if (after >= before) return false;
            for (int j = 0; j < phases.Length; j++)
            {
                phaseError[phases[j]] = _trialErrors[j];
                CopyScores(data, phases[j], _scratch, _scores);
            }
            return true;
        }

        #endregion

        #region Parameters

        private static readonly HashSet<string> _unmappedScalars = new();

        private static ValueRange PstRange(int piece)
        {
            var r = _cfg.Bounds.PstRanges;
            var range = piece switch
            {
                Pieces.WhitePawn => r.Pawn,
                Pieces.WhiteKnight => r.Knight,
                Pieces.WhiteBishop => r.Bishop,
                Pieces.WhiteRook => r.Rook,
                Pieces.WhiteQueen => r.Queen,
                Pieces.WhiteKing => r.King,
                _ => null
            };
            return range ?? _cfg.PstRange;
        }

        private static int ScalarMin(string property) =>
            _cfg.Bounds.ScalarRanges.TryGetValue(property, out var range) ? range.Min : _cfg.ScalarMinValue;

        private static int ScalarMax(string property)
        {
            var b = _cfg.Bounds;
            if (b.ScalarRanges.TryGetValue(property, out var range)) return range.Max;
            if (b.ScalarGroupMap.TryGetValue(property, out var group) && b.ScalarGroups.TryGetValue(group, out var max))
                return max;
            if (_unmappedScalars.Add(property))
                Console.WriteLine($"[Bounds] Scalar '{property}' has no group, using ScalarMaxValue {b.ScalarMaxValue}");
            return b.ScalarMaxValue;
        }

        private static Func<int, bool> ScalarOrderValidator(string property, object board)
        {
            var rules = _cfg.Bounds.ScalarOrderRules;
            if (!rules.Any(r => r.Lower == property || r.Upper == property)) return null;

            int Read(string name) => Convert.ToInt32(typeof(BoardEvaluation).GetProperty(name).GetValue(board));

            return v =>
            {
                foreach (var r in rules)
                {
                    if (r.Lower == property && v >= Read(r.Upper)) return false;
                    if (r.Upper == property && Read(r.Lower) >= v) return false;
                }
                return true;
            };
        }

        private static int KingSafetyMax(string property, int fallback)
        {
            var b = _cfg.Bounds;
            return b.KingSafetyGroupMap.TryGetValue(property, out var group) && b.KingSafetyGroups.TryGetValue(group, out var max)
                ? max : fallback;
        }

        // Shield bonuses must not increase with distance: 2 >= 3 >= 4.
        private static (string Prev, string Next) ShieldNeighbours(string name) => name switch
        {
            "PawnShield2Value" => (null, "PawnShield3Value"),
            "PawnShield3Value" => ("PawnShield2Value", "PawnShield4Value"),
            "PawnShield4Value" => ("PawnShield3Value", null),
            "PawnKingShield2Value" => (null, "PawnKingShield3Value"),
            "PawnKingShield3Value" => ("PawnKingShield2Value", "PawnKingShield4Value"),
            "PawnKingShield4Value" => ("PawnKingShield3Value", null),
            _ => (null, null)
        };

        private static List<Parameter> BuildParameters()
        {
            var list = new List<Parameter>();
            var staticEvaluation = (StaticEvaluation)_configuration.Evaluation.Static;

            for (byte phase = 0; phase < 2; phase++)
            {
                byte ph = phase;
                string phaseName = PhaseNames[ph];
                if (_cfg.TuneScalars)
                {
                    var board = staticEvaluation.GetBoard(ph);
                    foreach (var property in typeof(BoardEvaluation).GetProperties())
                    {
                        if (property.PropertyType == typeof(short) || property.PropertyType == typeof(int))
                        {
                            if (ph == Phase.End && property.Name.EndsWith("OutpostValue")) continue;
                            var prop = property;
                            list.Add(new Parameter
                            {
                                Name = $"{phaseName}.{prop.Name}",
                                Phase = ph,
                                Min = ScalarMin(prop.Name),
                                Max = ScalarMax(prop.Name),
                                IsValid = ScalarOrderValidator(prop.Name, board),
                                JsonPath = $"Evaluation.Static.{phaseName}.{prop.Name}",
                                Get = () => Convert.ToInt32(prop.GetValue(board)),
                                Set = v => prop.SetValue(board, prop.PropertyType == typeof(short) ? (object)(short)v : v)
                            });
                        }
                        else if (property.PropertyType == typeof(short[]) || property.PropertyType == typeof(byte[]))
                        {
                            var array = (Array)property.GetValue(board);
                            var isShort = property.PropertyType == typeof(short[]);
                            for (int n = 0; n < array.Length; n++)
                            {
                                int index = n;
                                var name = property.Name;
                                list.Add(new Parameter
                                {
                                    Name = $"{phaseName}.{name}[{index}]",
                                    Phase = ph,
                                    Min = ScalarMin(name),
                                    Max = ScalarMax(name),
                                    JsonPath = $"Evaluation.Static.{phaseName}.{name}[{index}]",
                                    Get = () => Convert.ToInt32(array.GetValue(index)),
                                    Set = v => array.SetValue(isShort ? (object)(short)v : (byte)v, index)
                                });
                            }
                        }
                    }
                }

                if (_cfg.TunePieceValues)
                {
                    var pieces = (PieceEvaluation)_configuration.Evaluation.GetPiece(ph);
                    var ranges = _cfg.Bounds.PieceRanges;
                    AddPiece(list, ph, phaseName, "Knight", Pieces.WhiteKnight, ranges.Knight, () => pieces.Knight, v => pieces.Knight = (short)v);
                    AddPiece(list, ph, phaseName, "Bishop", Pieces.WhiteBishop, ranges.Bishop, () => pieces.Bishop, v => pieces.Bishop = (short)v);
                    AddPiece(list, ph, phaseName, "Rook", Pieces.WhiteRook, ranges.Rook, () => pieces.Rook, v => pieces.Rook = (short)v);
                    AddPiece(list, ph, phaseName, "Queen", Pieces.WhiteQueen, ranges.Queen, () => pieces.Queen, v => pieces.Queen = (short)v);
                }

                if (_cfg.TunePst)
                {
                    for (byte piece = 0; piece < 6; piece++)
                    {
                        var table = _collection.Values[piece].Values[ph];
                        var mirrorTable = _collection.Values[piece + 6].Values[ph];
                        bool isPawn = piece == Pieces.WhitePawn || piece == Pieces.BlackPawn;
                        int pieceIndex = piece;

                        foreach (var square in table.Values.Keys.ToList())
                        {
                            if (isPawn && (square.EndsWith('1') || square.EndsWith('8'))) continue;

                            var sq = square;
                            byte sqIndex = SquareIndex[sq];
                            list.Add(new Parameter
                            {
                                Name = $"PST.{pieceIndex}.{phaseName}.{sq}",
                                Phase = ph,
                                Min = PstRange(pieceIndex).Min,
                                Max = PstRange(pieceIndex).Max,
                                LinearPiece = pieceIndex,
                                WhiteMask = 1UL << sqIndex,
                                BlackMask = 1UL << (sqIndex ^ 56),
                                Get = () => table.Values[sq],
                                Set = v =>
                                {
                                    table.Values[sq] = (short)v;
                                    mirrorTable.Values[MirrorSquare(sq)] = (short)v;
                                }
                            });
                        }
                    }
                }
            }

            AddKingSafety(list, staticEvaluation);
            ApplyPieceRules(list);

            return list;
        }

        private static void AddKingSafety(List<Parameter> list, StaticEvaluation staticEvaluation)
        {
            if (_cfg.TuneKingSafety)
            {
                AddObject(list, staticEvaluation.KingSafety, "Evaluation.Static.KingSafety", "KingSafety", 1, 0, _cfg.KingSafetyMaxValue);
            }

            if (_cfg.TunePassedPawns)
            {
                AddPassedPawns(list, staticEvaluation.PassedPawnConfiguration, "PassedPawnConfiguration");
                AddPassedPawns(list, staticEvaluation.ProtectedPassedPawnConfiguration, "ProtectedPassedPawnConfiguration");
                AddPassedPawns(list, staticEvaluation.ConnectedPassedPawnConfiguration, "ConnectedPassedPawnConfiguration");
            }
        }

        private static void AddPassedPawns(List<Parameter> list, object config, string name)
        {
            foreach (var property in config.GetType().GetProperties())
            {
                if (property.PropertyType != typeof(byte[]) || property.Name.StartsWith("Black")) continue;

                byte phase = property.Name.EndsWith("Middle") ? (byte)0 : (byte)1;
                var array = (byte[])property.GetValue(config);
                var blackArray = (byte[])config.GetType().GetProperty("Black" + property.Name.Substring(5)).GetValue(config);
                for (int n = 1; n < array.Length - 1; n++)
                {
                    int index = n;
                    list.Add(new Parameter
                    {
                        Name = $"{name}.{property.Name}[{index}]",
                        Phase = phase,
                        Min = 0,
                        Max = name == "PassedPawnConfiguration" ? _cfg.PassedPawnMaxValue : Math.Min(_cfg.PassedPawnMaxValue, _cfg.Bounds.SupportPassedPawnMaxValue),
                        IsValid = v => (index == 1 || v >= array[index - 1]) && (index == array.Length - 2 || v <= array[index + 1]),
                        JsonPath = $"Evaluation.Static.{name}.{property.Name}[{index}]",
                        MirrorJsonPath = $"Evaluation.Static.{name}.Black{property.Name.Substring(5)}[{array.Length - 1 - index}]",
                        Get = () => array[index],
                        Set = v =>
                        {
                            array[index] = (byte)v;
                            blackArray[array.Length - 1 - index] = (byte)v;
                        }
                    });
                }
            }
        }

        private static void AddObject(List<Parameter> list, object target, string jsonPath, string name, byte phase, int min, int max)
        {
            foreach (var property in target.GetType().GetProperties())
            {
                var prop = property;
                var type = prop.PropertyType;
                string childName = $"{name}.{prop.Name}";
                string childPath = $"{jsonPath}.{prop.Name}";

                if (prop.Name.EndsWith("MaxDistance")) continue;
                if (_cfg.Bounds.FrozenKingSafety.Contains(prop.Name)) continue;
                int propMax = KingSafetyMax(prop.Name, max);
                var (shieldPrev, shieldNext) = ShieldNeighbours(prop.Name);

                if (type == typeof(byte) || type == typeof(short) || type == typeof(int))
                {
                    if (target is BlockadeConfiguration && prop.Name.StartsWith("Black")) continue;
                    var mirrorProp = target is BlockadeConfiguration ? target.GetType().GetProperty("Black" + prop.Name.Substring(5)) : null;
                    bool trofism = prop.Name == "TrofismCoefficientValue";
                    list.Add(new Parameter
                    {
                        Name = childName,
                        Phase = phase,
                        Phases = new byte[] { 0, 1 },
                        Min = min,
                        Max = propMax,
                        JsonPath = childPath,
                        MirrorJsonPath = mirrorProp != null ? $"{jsonPath}.{mirrorProp.Name}" : null,
                        IsValid = shieldPrev == null && shieldNext == null ? null : (Func<int, bool>)(v =>
                            (shieldPrev == null || v <= Convert.ToInt32(target.GetType().GetProperty(shieldPrev).GetValue(target)))
                            && (shieldNext == null || v >= Convert.ToInt32(target.GetType().GetProperty(shieldNext).GetValue(target)))),
                        Get = () => Convert.ToInt32(prop.GetValue(target)),
                        Set = v =>
                        {
                            prop.SetValue(target, Convert.ChangeType(v, type));
                            mirrorProp?.SetValue(target, Convert.ChangeType(v, type));
                            if (trofism) _board.SetTrofismCoefficient(v);
                        }
                    });
                }
                else if (type == typeof(byte[]) || type == typeof(short[]) || type == typeof(int[]))
                {
                    var array = (Array)prop.GetValue(target);
                    var elementType = type.GetElementType();
                    for (int n = 0; n < array.Length; n++)
                    {
                        int index = n;
                        bool pieceAttack = prop.Name == "PieceAttackValue";
                        if (pieceAttack && (index == 0 || index == array.Length - 1)) continue;
                        int At(int i) => Convert.ToInt32(array.GetValue(i));
                        Func<int, bool> pieceAttackValid = v => index switch
                        {
                            1 or 2 => v <= At(3),
                            3 => v >= Math.Max(At(1), At(2)) && v <= At(4),
                            _ => v >= At(3)
                        };
                        list.Add(new Parameter
                        {
                            Name = $"{childName}[{index}]",
                            Phase = phase,
                            Phases = new byte[] { 0, 1 },
                            Min = min,
                            Max = propMax,
                            JsonPath = $"{childPath}[{index}]",
                            Get = () => Convert.ToInt32(array.GetValue(index)),
                            Set = v => array.SetValue(Convert.ChangeType(v, elementType), index),
                            IsValid = pieceAttack ? pieceAttackValid : prop.Name == "AttackWeight"
                                ? (Func<int, bool>)(v => (index == 0 || (v >= Convert.ToInt32(array.GetValue(index - 1))
                                        && v - Convert.ToInt32(array.GetValue(index - 1)) <= _cfg.Bounds.MaxAttackWeightStep))
                                    && (index == array.Length - 1 || (v <= Convert.ToInt32(array.GetValue(index + 1))
                                        && Convert.ToInt32(array.GetValue(index + 1)) - v <= _cfg.Bounds.MaxAttackWeightStep)))
                                : null
                        });
                    }
                }
                else if (type.IsClass && type != typeof(string))
                {
                    var child = prop.GetValue(target);
                    if (child != null) AddObject(list, child, childPath, childName, phase, min, max);
                }
            }
        }

        // Raw piece value may differ from the effective value (piece + PST mean) by up to the PST bound.
        private static void AddPiece(List<Parameter> list, byte phase, string phaseName, string piece, byte pieceIndex, ValueRange range, Func<int> get, Action<int> set)
        {
            list.Add(new Parameter
            {
                Name = $"{phaseName}.Piece.{piece}",
                Phase = phase,
                Min = range.Min - PstRange(pieceIndex).Max,
                Max = range.Max + PstRange(pieceIndex).Max,
                LinearPiece = pieceIndex,
                WhiteMask = ulong.MaxValue,
                BlackMask = ulong.MaxValue,
                JsonPath = $"Evaluation.{phaseName}.{piece}",
                Get = get,
                Set = set
            });
        }

        #endregion

        #region Tuning

        private static void Tune(TexelDataSet data, List<Parameter> parameters, double k)
        {
            var phaseError = InitScores(data, k);

            var watch = Stopwatch.StartNew();
            long lastReport = 0;

            foreach (var step in _cfg.StepSizes)
            {
                _stepIndex = Array.IndexOf(_cfg.StepSizes, step);
                if (_stepIndex < _resumeStepIndex) continue;

                for (int epoch = 0; epoch < _cfg.MaxEpochsPerStep; epoch++)
                {
                    double epochStart = phaseError.Sum();
                    int improvedCount = 0;
                    int done = 0;

                    foreach (var p in parameters)
                    {
                        done++;
                        if (watch.ElapsedMilliseconds - lastReport >= _cfg.ProgressIntervalSeconds * 1000L)
                        {
                            lastReport = watch.ElapsedMilliseconds;
                            Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep}: {100.0 * done / parameters.Count:F1}% ({done}/{parameters.Count}) improved {improvedCount}, error {phaseError.Sum() / data.TrainCount:F8}, {watch.Elapsed:hh\\:mm\\:ss}");
                        }

                        if (p.PhaseSet.All(ph => data.PhaseIndices[ph].Length == 0)) continue;

                        int original = p.Get();
                        bool improved = false;

                        foreach (var direction in Directions)
                        {
                            int candidate = original + direction * step;
                            if (candidate < p.Min || candidate > p.Max) continue;

                            if (TryCandidate(p, candidate, data, k, phaseError))
                            {
                                improved = true;
                                improvedCount++;
                                break;
                            }
                        }

                        if (!improved && p.Get() != original) Apply(p, original);
                    }

                    if (_useIncremental) Resync(data, k, phaseError);
                    double total = phaseError.Sum() / data.TrainCount;
                    Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep} finished: improved {improvedCount}, error {total:F8}, validation {ValidationError(data, k):F8}, {watch.Elapsed:hh\\:mm\\:ss}");

                    if (improvedCount == 0) break;
                    Save(parameters);
                    if ((epochStart - phaseError.Sum()) / data.TrainCount < _cfg.MinEpochImprovementFor(_stepIndex)) break;
                }

                _stepIndex++;
                Save(parameters);
            }
        }

        private static string MirrorSquare(string square) => square[0].ToString() + (9 - (square[1] - '0'));

        // Moves each table's mean into its piece value; evaluation (piece value + table cell) is unchanged.
        private static void Recentre()
        {
            for (byte ph = 0; ph < 2; ph++)
            {
                var pieces = (PieceEvaluation)_configuration.Evaluation.GetPiece(ph);
                for (byte piece = 1; piece <= 4; piece++)
                {
                    var white = _collection.Values[piece].Values[ph].Values;
                    var black = _collection.Values[piece + 6].Values[ph].Values;
                    int mean = (int)Math.Round(white.Values.Average(v => (double)v));
                    if (mean == 0) continue;

                    foreach (var key in white.Keys.ToList()) white[key] = (short)(white[key] - mean);
                    foreach (var key in black.Keys.ToList()) black[key] = (short)(black[key] - mean);

                    switch (piece)
                    {
                        case 1: pieces.Knight = (short)(pieces.Knight + mean); break;
                        case 2: pieces.Bishop = (short)(pieces.Bishop + mean); break;
                        case 3: pieces.Rook = (short)(pieces.Rook + mean); break;
                        case 4: pieces.Queen = (short)(pieces.Queen + mean); break;
                    }
                }
            }

            for (byte phase = 0; phase < 2; phase++)
                _factory.GetEvaluationService(phase).Reload(_configuration, _staticValueProvider);
            _board.RebuildKingShieldLookup();
            _board.RefreshPieceValues();
            _board.ResetEvaluationCaches();
            Console.WriteLine("Tables re-centred into piece values");
        }

        #endregion

        #region Output

        private static int _stepIndex;
        private static int _resumeStepIndex;

        private static string CheckpointPath => Path.Combine(_cfg.OutputDirectory, "checkpoint.txt");

        private static string ParameterSignature(List<Parameter> parameters)
        {
            ulong h = 1469598103934665603UL;
            foreach (var p in parameters)
            {
                foreach (char ch in p.Name) h = (h ^ ch) * 1099511628211UL;
                h = (h ^ '|') * 1099511628211UL;
            }
            return h.ToString("X16");
        }

        private static void WriteCheckpoint(List<Parameter> parameters)
        {
            var tmp = CheckpointPath + ".tmp";
            File.WriteAllText(tmp, _stepIndex + "\n" + string.Join(",", GetValues(parameters)) + "\n" + ParameterSignature(parameters));
            File.Move(tmp, CheckpointPath, true);
        }

        private static void TryResume(List<Parameter> parameters)
        {
            if (!File.Exists(CheckpointPath)) return;

            var lines = File.ReadAllLines(CheckpointPath);
            if (lines.Length < 2 || !int.TryParse(lines[0], out var stepIndex)) return;

            var values = lines[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            if (values.Length != parameters.Count)
            {
                Console.WriteLine($"Checkpoint ignored: {values.Length} values, {parameters.Count} parameters");
                return;
            }
            if (lines.Length < 3 || lines[2] != ParameterSignature(parameters))
            {
                Console.WriteLine("Checkpoint ignored: parameter list changed");
                return;
            }

            ApplyAll(parameters, values);
            _resumeStepIndex = stepIndex;
            Console.WriteLine($"Resumed from checkpoint at step index {stepIndex}");
        }

        private static void Save(List<Parameter> parameters)
        {
            Directory.CreateDirectory(_cfg.OutputDirectory);
            WriteCheckpoint(parameters);

            var root = new JObject { ["Evaluation"] = JObject.Parse(File.ReadAllText(ConfigurationLoader.EvaluationFile)) };
            int missing = 0;

            foreach (var p in parameters)
            {
                if (p.JsonPath == null) continue;

                var token = root.SelectToken(p.JsonPath);
                if (token == null) { missing++; continue; }
                var value = p.Get();
                token.Replace(new JValue(value));

                if (p.MirrorJsonPath == null) continue;
                var mirror = root.SelectToken(p.MirrorJsonPath);
                if (mirror == null) { missing++; continue; }
                mirror.Replace(new JValue(value));
            }

            File.WriteAllText(Path.Combine(_cfg.OutputDirectory, _cfg.TunedConfigurationFile), root["Evaluation"].ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(_cfg.OutputDirectory, _cfg.TunedStaticTablesFile), JsonConvert.SerializeObject(_collection, Formatting.Indented));

            Console.WriteLine($"Saved to {Path.GetFullPath(_cfg.OutputDirectory)} (config paths not found: {missing})");
        }

        #endregion
    }
}
