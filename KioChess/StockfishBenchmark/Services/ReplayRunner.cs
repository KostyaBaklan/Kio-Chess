using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Services;
using StockfishBenchmark.Models;
using System.Diagnostics;

namespace StockfishBenchmark.Services;

public class ReplayRunner : IReplayRunner
{
    public async Task<List<ComparisonRow>> CompareAsync(
        BenchmarkResult baseline,
        IProgress<ComparisonRow> progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ReplayCore(baseline, progress, cancellationToken), cancellationToken);
    }

    private List<ComparisonRow> ReplayCore(
        BenchmarkResult baseline,
        IProgress<ComparisonRow> progress,
        CancellationToken cancellationToken)
    {
        var rows = new List<ComparisonRow>();
        var session = baseline.Session;

        var moveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
        var position = new Position();
        var strategyFactory = ContainerLocator.Current.Resolve<IStrategyFactory>();
        var strategy = strategyFactory.GetStrategy(session.Depth, position, session.Strategy);

        bool diverged = false;
        int engineMovesReplayed = 0;

        foreach (var saved in baseline.Moves)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Stop once we have replayed the same number of engine moves as the baseline session
            if (saved.IsEngineMove && engineMovesReplayed >= session.MoveCount)
                break;

            var row = new ComparisonRow
            {
                MoveNumber         = saved.MoveNumber,
                UciMove            = saved.UciMove,
                MoveNotation       = saved.MoveNotation,
                BaselineDurationMs = saved.DurationMs,
                BaselineTtCount    = saved.TtCount,
                BaselineMemoryMB   = saved.ProcessMemoryMB,
                IsDiverged         = diverged
            };

            if (!diverged && saved.IsEngineMove)
            {
                // Re-run the engine and measure
                var sw = Stopwatch.StartNew();
                var result = strategy.GetResult();
                sw.Stop();

                row.NewDurationMs = sw.ElapsedMilliseconds;
                row.NewTtCount    = strategy.Size;
                row.NewMemoryMB   = Process.GetCurrentProcess().WorkingSet64 / 1_048_576.0;

                bool regression = result.Move?.Key != saved.MoveKey || saved.TtCount!= row.NewTtCount;
                row.IsRegression = regression;

                // Apply the move chosen by the new engine; fall back to baseline key if null
                MoveBase moveToApply = result.Move ?? moveProvider.Get(saved.MoveKey);

                if (moveToApply != null)
                {
                    row.MoveNotation = moveToApply.ToLightString();
                    ApplyMove(position, moveToApply);
                    engineMovesReplayed++;

                    if (regression)
                        diverged = true;
                }
                else
                {
                    diverged = true;
                }
            }
            else if (!diverged && !saved.IsEngineMove)
            {
                // Opponent ply — replay the saved move by key
                row.NewDurationMs = 0;
                row.NewTtCount    = 0;
                row.NewMemoryMB   = Process.GetCurrentProcess().WorkingSet64 / 1_048_576.0;

                MoveBase moveToApply = moveProvider.Get(saved.MoveKey);
                if (moveToApply != null)
                    ApplyMove(position, moveToApply);
                else
                    diverged = true;
            }
            else
            {
                // Already diverged — emit stub row with no new timing
                row.NewDurationMs = 0;
                row.NewTtCount    = 0;
                row.NewMemoryMB   = 0;
            }

            rows.Add(row);
            progress?.Report(row);
        }

        return rows;
    }

    private static void ApplyMove(Position position, MoveBase move)
    {
        if (position.GetHistory().Any())
            position.Make(move);
        else
            position.MakeFirst(move);
    }
}
