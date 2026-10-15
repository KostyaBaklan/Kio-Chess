using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using Newtonsoft.Json;
using StockfishBenchmark.Models;
using StockFishCore.Stockfish;
using System.Diagnostics;
using System.IO;

namespace StockfishBenchmark.Services;

/// <summary>
/// Separate from BenchmarkRunner/ReplayRunner (which stay unchanged for performance work).
/// Run: Kio vs deterministic Stockfish, recording null-move statistics per engine move.
/// Compare: forced-path replay of a saved null run (no divergence); the current build searches every
/// engine position of the baseline game and is compared per position.
/// </summary>
public class NullBenchmarkRunner
{
    private const string StockfishPath =
        @"..\..\..\stockfish\stockfish-windows-x86-64-avx2.exe";

    private const string Folder = "NullBenchmarks";

    public Task<NullBenchmarkResult> RunAsync(BenchmarkSession session,
        IProgress<NullMoveRecord> progress, CancellationToken ct)
        => Task.Run(() => Run(session, progress, ct), ct);

    public Task<NullBenchmarkResult> CompareAsync(NullBenchmarkResult baseline,
        IProgress<NullMoveRecord> progress, CancellationToken ct)
        => Task.Run(() => Compare(baseline, progress, ct), ct);

    private static NullBenchmarkResult Run(BenchmarkSession session,
        IProgress<NullMoveRecord> progress, CancellationToken ct)
    {
        var records = new List<NullMoveRecord>();
        var stockfish = new Stockfish(StockfishPath, session.StockfishDepth, session.StockfishElo);
        var position = new Position();
        var factory = ContainerLocator.Current.Resolve<IStrategyFactory>();
        var strategy = factory.GetStrategy(session.Depth, position, session.Strategy);
        var endGameStrategy = factory.GetStrategy(2, position, "ab");

        bool isStockfishTurn = session.Color == "w";
        int engineMoves = 0, ply = 0;
        IResult result = new Result();
        string endReason = null;

        while (result.GameResult == GameResult.Continue && engineMoves < session.MoveCount
               && !ct.IsCancellationRequested)
        {
            ply++;
            var fen = stockfish.GetFenPosition();
            var record = new NullMoveRecord { MoveNumber = ply };

            if (isStockfishTurn)
            {
                var uci = stockfish.GetBestMove();
                var legal = position.GetAllMoves();
                MoveBase chosen = null;
                foreach (var m in legal)
                    if (m.ToUciString() == uci) { chosen = m; break; }

                if (chosen == null)
                {
                    if (legal == null || legal.Count == 0)
                    {
                        result = endGameStrategy.GetResult(short.MinValue, short.MaxValue, 1);
                        endReason = Describe(result.GameResult, false);
                    }
                    else
                    {
                        endReason = $"Stopped: Stockfish move '{uci}' not found among Kio legal moves";
                    }
                    break;
                }

                Apply(position, chosen);
                stockfish.SetPosition(fen, chosen.ToUciString());
                Fill(record, chosen);
            }
            else
            {
                engineMoves++;
                record.IsEngineMove = true;

                NullStats.Reset();
                var sw = Stopwatch.StartNew();
                result = strategy.GetResult();
                sw.Stop();
                var snap = NullStats.Snapshot();

                if (result.Move == null)
                {
                    endReason = result.GameResult == GameResult.Continue
                        ? "Stopped: Kio returned no move"
                        : Describe(result.GameResult, true);
                    break;
                }

                Apply(position, result.Move);
                stockfish.SetPosition(fen, result.Move.ToUciString());
                Fill(record, result.Move);
                record.Value = result.Value;
                record.DurationMs = sw.Elapsed.TotalMilliseconds;
                FillStats(record, snap);
            }

            progress?.Report(record);
            records.Add(record);
            isStockfishTurn = !isStockfishTurn;
        }

        if (endReason == null)
        {
            if (ct.IsCancellationRequested)
            {
                endReason = "Cancelled";
            }
            else
            {
                // Move limit reached: the last Kio move may have ended the game.
                var next = position.GetAllMoves();
                if (next == null || next.Count == 0)
                {
                    var final = endGameStrategy.GetResult(short.MinValue, short.MaxValue, 1);
                    endReason = Describe(final.GameResult, isStockfishTurn);
                }
                else
                {
                    endReason = $"Move limit reached ({session.MoveCount} Kio moves)";
                }
            }
        }

        return new NullBenchmarkResult
        {
            NullStatsEnabled = NullStats.Enabled,
            EndReason = endReason,
            Session = session,
            Moves = records,
            Summary = NullBenchmarkSummary.Compute(records, false)
        };
    }

    private static string Describe(GameResult r, bool kioToMove) => r switch
    {
        GameResult.Mate => kioToMove ? "Checkmate: Stockfish mated Kio (loss)" : "Checkmate: Kio mated Stockfish (win)",
        GameResult.Pat => "Draw: stalemate",
        GameResult.ThreefoldRepetition => "Draw: threefold repetition",
        GameResult.FiftyMoves => "Draw: fifty-move rule",
        GameResult.Draw => "Draw: insufficient material",
        _ => "Stopped: " + r
    };

    private static NullBenchmarkResult Compare(NullBenchmarkResult baseline,
        IProgress<NullMoveRecord> progress, CancellationToken ct)
    {
        var session = baseline.Session;
        var records = new List<NullMoveRecord>();
        var provider = ContainerLocator.Current.Resolve<MoveProvider>();
        var position = new Position();
        var factory = ContainerLocator.Current.Resolve<IStrategyFactory>();
        var strategy = factory.GetStrategy(session.Depth, position, session.Strategy);

        foreach (var saved in baseline.Moves)
        {
            ct.ThrowIfCancellationRequested();

            var record = new NullMoveRecord
            {
                MoveNumber = saved.MoveNumber,
                IsEngineMove = saved.IsEngineMove,
                MoveKey = saved.MoveKey,
                UciMove = saved.UciMove,
                MoveNotation = saved.MoveNotation
            };

            if (saved.IsEngineMove)
            {
                NullStats.Reset();
                var sw = Stopwatch.StartNew();
                var result = strategy.GetResult();
                sw.Stop();
                var snap = NullStats.Snapshot();

                record.HasBaseline = true;
                record.BaselineMoveKey = saved.MoveKey;
                record.BaselineMove = saved.MoveNotation;
                record.BaselineValue = saved.Value;
                record.BaselineDurationMs = saved.DurationMs;
                record.BaselineNullAttempts = saved.NullAttempts;
                record.BaselineNullFailHighs = saved.NullFailHighs;
                record.BaselineNullNodes = saved.NullNodes;
                record.BaselineNullMs = saved.NullMs;

                // new engine's own choice is reported; the baseline move is applied (forced path)
                if (result.Move != null)
                {
                    record.MoveKey = result.Move.Key;
                    record.UciMove = result.Move.ToUciString();
                    record.MoveNotation = result.Move.ToLightString();
                }
                record.Value = result.Value;
                record.DurationMs = sw.Elapsed.TotalMilliseconds;
                FillStats(record, snap);
            }

            var toApply = provider.Get(saved.MoveKey);
            if (toApply == null) break;
            Apply(position, toApply);

            progress?.Report(record);
            records.Add(record);
        }

        return new NullBenchmarkResult
        {
            NullStatsEnabled = NullStats.Enabled,
            EndReason = baseline.EndReason == null ? null : "Baseline: " + baseline.EndReason,
            Session = session,
            Moves = records,
            Summary = NullBenchmarkSummary.Compute(records, true)
        };
    }

    private static void Apply(Position position, MoveBase move)
    {
        if (position.GetHistory().Any()) position.Make(move);
        else position.MakeFirst(move);
    }

    private static void Fill(NullMoveRecord r, MoveBase move)
    {
        r.MoveKey = move.Key;
        r.UciMove = move.ToUciString();
        r.MoveNotation = move.ToLightString();
    }

    private static void FillStats(NullMoveRecord r, NullStatsSnapshot s)
    {
        r.NullAttempts = s.Attempts;
        r.NullFailHighs = s.FailHighs;
        r.NullNodes = s.Nodes;
        r.NullMs = s.Ms;
        r.DrawHits = s.DrawHits;
        r.RootNoMoves = s.RootNoMoves;
        r.DepthAttempts = s.DepthAttempts;
        r.DepthFailHighs = s.DepthFailHighs;
    }

    public static string GetDefaultPath(BenchmarkSession s)
    {
        Directory.CreateDirectory(Folder);
        return Path.Combine(Folder, $"null_{s.Strategy}_d{s.Depth}_{s.Color}_{s.RunDate:yyyyMMdd_HHmmss}.json");
    }

    public static void Save(NullBenchmarkResult result, string path)
        => File.WriteAllText(path, JsonConvert.SerializeObject(result, Formatting.Indented));

    public static NullBenchmarkResult Load(string path)
        => JsonConvert.DeserializeObject<NullBenchmarkResult>(File.ReadAllText(path));
}
