using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Pgn.Models;
using Engine.Pgn.Streaming;
using Engine.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;

namespace TexelTool;

internal sealed class Parameter
{
    public string Name;
    public byte Phase;
    public int Min;
    public int Max;
    public Func<int> Get;
    public Action<int> Set;
    public string JsonPath;
}

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

    private static readonly string[] PhaseNames = ["Opening", "Middle", "End"];

    private static void Main(string[] args)
    {
        bool worker = args.Length >= 4 && args[0] == "--worker";

        try
        {
            _cfg = JsonConvert.DeserializeObject<TexelConfiguration>(File.ReadAllText(@"Config\TexelConfiguration.json"));

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
                RunWorker(int.Parse(args[1]), int.Parse(args[2]), args[3]);
                return;
            }

            TexelDataSet data;
            double k;

            if (_cfg.ReuseDataset && File.Exists(_cfg.DatasetFile))
            {
                data = TexelDataSet.Load(_cfg.DatasetFile, out k);
                Console.WriteLine($"Loaded {data.Count} positions from {_cfg.DatasetFile}, K = {k:F4}");
            }
            else
            {
                data = Extract(position);
                data.Freeze();

                if (_cfg.DeletePgnAfterExtraction)
                {
                    File.Delete(_cfg.PgnPath);
                    Console.WriteLine($"Deleted {_cfg.PgnPath}");
                }
                Console.WriteLine($"Extracted {data.Count} positions");

                FilterByScore(data);
                Console.WriteLine($"After score filter: {data.Count} (Opening {data.PhaseIndices[0].Length}, Middle {data.PhaseIndices[1].Length}, End {data.PhaseIndices[2].Length})");

                var baseScores = ScoreAll(data);
                k = FitK(data, baseScores);
                Console.WriteLine($"K = {k:F4}, error = {TotalError(data, baseScores, k):F6}");

                data.Save(_cfg.DatasetFile, k);
                Console.WriteLine($"Dataset saved to {Path.GetFullPath(_cfg.DatasetFile)}");
            }

            var parameters = BuildParameters();
            Console.WriteLine($"Parameters: {parameters.Count}");

            if (_cfg.WorkerCount > 1)
            {
                TuneParallel(parameters, Path.GetFullPath(_cfg.DatasetFile));
            }
            else
            {
                Tune(data, parameters, k);
            }

            var scores = ScoreAll(data);
            Console.WriteLine($"Final error = {TotalError(data, scores, k):F6}");

            Save(parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            _appDbService?.Disconnect();
        }

        if (!worker) Console.ReadLine();
    }

    #region Extraction

    private static TexelDataSet Extract(Position position)
    {
        var data = new TexelDataSet();
        int games = 0, failures = 0, read = 0;
        var watch = Stopwatch.StartNew();
        long lastReport = 0;

        using var reader = new PgnStreamReader(_cfg.PgnPath);

        foreach (var game in reader.ReadGames())
        {
            if (games >= _cfg.MaxGames || data.Count >= _cfg.MaxPositions) break;

            read++;
            if (watch.ElapsedMilliseconds - lastReport >= _cfg.ProgressIntervalSeconds * 1000L)
            {
                lastReport = watch.ElapsedMilliseconds;
                Console.WriteLine($"[Extract] {reader.ProgressPercent:F1}% of file | read {read}, used {games}/{_cfg.MaxGames} games | positions {data.Count}/{_cfg.MaxPositions} | failures {failures} | {watch.Elapsed:hh\\:mm\\:ss}");
            }

            var result = ParseResult(game);
            if (result < 0) continue;
            if (!PassesElo(game)) continue;

            games++;
            int made = 0;
            try
            {
                bool isWhite = true;
                foreach (var move in game.Moves.Take(_cfg.MaxPliesPerGame))
                {
                    TexelPgnConverter.ProcessMove(position, move, isWhite);
                    made++;
                    isWhite = !isWhite;

                    if (made < _cfg.SkipOpeningPlies || (made - _cfg.SkipOpeningPlies) % _cfg.SampleEveryNthPosition != 0) continue;
                    if (_cfg.ExcludeCaptures && move.IsCapture) continue;
                    if (_cfg.ExcludeInCheck && (_board.IsCheckToWhite() || _board.IsCheckToBlack())) continue;

                    data.Add(_board, DetectPhase(_board, made), _history.GetCastleRights(), result);
                    if (data.Count >= _cfg.MaxPositions) break;
                }
            }
            catch (Exception)
            {
                failures++;
            }
            finally
            {
                for (int i = 0; i < made; i++) position.UnMake();
            }

            }

            Console.WriteLine($"[Extract] done: read {read}, used {games} games, positions {data.Count}, failures {failures}, {watch.Elapsed:hh\\:mm\\:ss}");
            return data;
    }

    /// <summary>Deterministic phase from the position itself (independent of search history).</summary>
    private static byte DetectPhase(Board board, int plies)
    {
        if (board.IsEndGame()) return Phase.End;
        if (plies <= _cfg.OpeningMaxPly && board.GetMaterialPhase() >= _cfg.OpeningMinMaterial) return Phase.Opening;
        return Phase.Middle;
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

    private static bool PassesElo(PgnGame game)
    {
        if (game.Tags == null) return false;
        if (!game.Tags.TryGetValue("WhiteElo", out var w) || !int.TryParse(w, out var white)) return false;
        if (!game.Tags.TryGetValue("BlackElo", out var b) || !int.TryParse(b, out var black)) return false;
        return Math.Min(white, black) >= _cfg.MinElo;
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
        for (int i = 0; i < scores.Length; i++)
        {
            double d = data.Results[i] - Sigmoid(scores[i], k);
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
        return (a + b) / 2;
    }

    // Sum of squared errors over positions of one phase (only that phase's parameters change).
    private static double PhaseError(TexelDataSet data, int phase, double k)
    {
        var indices = data.PhaseIndices[phase];
        double sum = 0;
        for (int n = 0; n < indices.Length; n++)
        {
            int i = indices[n];
            double d = data.Results[i] - Sigmoid(ScoreOne(data, i), k);
            sum += d * d;
        }
        return sum;
    }

    private static void Apply(Parameter p, int value)
    {
        p.Set(value);
        _factory.GetEvaluationService(p.Phase).Reload(_configuration, _staticValueProvider);
        _board.ResetEvaluationCaches();
    }

    #endregion

    #region Parameters

    private static List<Parameter> BuildParameters()
    {
        var list = new List<Parameter>();
        var staticEvaluation = (StaticEvaluation)_configuration.Evaluation.Static;

        for (byte phase = 0; phase < 3; phase++)
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
                        var prop = property;
                        list.Add(new Parameter
                        {
                            Name = $"{phaseName}.{prop.Name}",
                            Phase = ph,
                            Min = _cfg.ScalarMinValue,
                            Max = _cfg.ScalarMaxValue,
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
                                Min = _cfg.ScalarMinValue,
                                Max = _cfg.ScalarMaxValue,
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
                AddPiece(list, ph, phaseName, "Knight", () => pieces.Knight, v => pieces.Knight = (short)v);
                AddPiece(list, ph, phaseName, "Bishop", () => pieces.Bishop, v => pieces.Bishop = (short)v);
                AddPiece(list, ph, phaseName, "Rook", () => pieces.Rook, v => pieces.Rook = (short)v);
                AddPiece(list, ph, phaseName, "Queen", () => pieces.Queen, v => pieces.Queen = (short)v);
            }

            if (_cfg.TunePst)
            {
                for (byte piece = 0; piece < 12; piece++)
                {
                    var table = _collection.Values[piece].Values[ph];
                    bool isPawn = piece == Pieces.WhitePawn || piece == Pieces.BlackPawn;
                    int pieceIndex = piece;

                    foreach (var square in table.Values.Keys.ToList())
                    {
                        if (isPawn && (square.EndsWith('1') || square.EndsWith('8'))) continue;

                        var sq = square;
                        list.Add(new Parameter
                        {
                            Name = $"PST.{pieceIndex}.{phaseName}.{sq}",
                            Phase = ph,
                            Min = _cfg.PstMinValue,
                            Max = _cfg.PstMaxValue,
                            Get = () => table.Values[sq],
                            Set = v => table.Values[sq] = (short)v
                        });
                    }
                }
            }
        }

        return list;
    }

    private static void AddPiece(List<Parameter> list, byte phase, string phaseName, string piece, Func<int> get, Action<int> set)
    {
        list.Add(new Parameter
        {
            Name = $"{phaseName}.Piece.{piece}",
            Phase = phase,
            Min = _cfg.PieceValueMin,
            Max = _cfg.PieceValueMax,
            JsonPath = $"Evaluation.{phaseName}.{piece}",
            Get = get,
            Set = set
        });
    }

    #endregion

    #region Tuning

    private static void Tune(TexelDataSet data, List<Parameter> parameters, double k)
    {
        var phaseError = new double[3];
        for (int phase = 0; phase < 3; phase++) phaseError[phase] = PhaseError(data, phase, k);

        var watch = Stopwatch.StartNew();
        long lastReport = 0;

        foreach (var step in _cfg.StepSizes)
        {
            for (int epoch = 0; epoch < _cfg.MaxEpochsPerStep; epoch++)
            {
                int improvedCount = 0;
                int done = 0;

                foreach (var p in parameters)
                {
                    done++;
                    if (watch.ElapsedMilliseconds - lastReport >= _cfg.ProgressIntervalSeconds * 1000L)
                    {
                        lastReport = watch.ElapsedMilliseconds;
                        Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep}: {100.0 * done / parameters.Count:F1}% ({done}/{parameters.Count}) improved {improvedCount}, error {phaseError.Sum() / data.Count:F6}, {watch.Elapsed:hh\\:mm\\:ss}");
                    }

                    if (data.PhaseIndices[p.Phase].Length == 0) continue;

                    int original = p.Get();
                    bool improved = false;

                    foreach (var direction in new[] { 1, -1 })
                    {
                        int candidate = original + direction * step;
                        if (candidate < p.Min || candidate > p.Max) continue;

                        Apply(p, candidate);
                        double error = PhaseError(data, p.Phase, k);

                        if (error < phaseError[p.Phase])
                        {
                            phaseError[p.Phase] = error;
                            improved = true;
                            improvedCount++;
                            break;
                        }
                    }

                    if (!improved) Apply(p, original);
                }

                double total = phaseError.Sum() / data.Count;
                Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep} finished: improved {improvedCount}, error {total:F6}, {watch.Elapsed:hh\\:mm\\:ss}");

                if (improvedCount == 0) break;
            }
        }
    }

    #endregion

    #region Output

    private static void Save(List<Parameter> parameters)
    {
        Directory.CreateDirectory(_cfg.OutputDirectory);

        var root = JObject.Parse(File.ReadAllText(@"Config\Configuration.json"));
        int missing = 0;

        foreach (var p in parameters)
        {
            if (p.JsonPath == null) continue;

            var token = root.SelectToken(p.JsonPath);
            if (token == null) { missing++; continue; }
            token.Replace(new JValue(p.Get()));
        }

        File.WriteAllText(Path.Combine(_cfg.OutputDirectory, _cfg.TunedConfigurationFile), root.ToString(Formatting.Indented));
        File.WriteAllText(Path.Combine(_cfg.OutputDirectory, _cfg.TunedStaticTablesFile), JsonConvert.SerializeObject(_collection, Formatting.Indented));

        Console.WriteLine($"Saved to {Path.GetFullPath(_cfg.OutputDirectory)} (config paths not found: {missing})");
    }

    #endregion
}
