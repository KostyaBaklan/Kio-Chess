using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using StockfishBenchmark.Models;
using System.Diagnostics;

namespace StockfishBenchmark.Services;

public class TtBenchmarkRunner : ITtBenchmarkRunner
{
    public Task<List<TtVariantResult>> RunAsync(
        BenchmarkResult line,
        IReadOnlyList<TtVariant> variants,
        IProgress<string> status = null,
        IProgress<TtVariantResult> completed = null,
        IProgress<TtMoveRecord> moves = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => RunCore(line, variants, status, completed, moves, cancellationToken), cancellationToken);
    }

    private static List<TtVariantResult> RunCore(
        BenchmarkResult line,
        IReadOnlyList<TtVariant> variants,
        IProgress<string> status,
        IProgress<TtVariantResult> completed,
        IProgress<TtMoveRecord> moves,
        CancellationToken cancellationToken)
    {
        var results = new List<TtVariantResult>();
        try
        {
            for (int i = 0; i < variants.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                status?.Report($"Variant {i + 1}/{variants.Count}: {variants[i].Name}");

                var result = RunVariant(line, variants[i], status, moves, cancellationToken);
                results.Add(result);
                completed?.Report(result);
            }
        }
        finally
        {
            TranspositionTableService.CapacityOverrideMB = 0;
            NullStrategy.CapacityOverrideMB = 0;
            ContainerLocator.Current.Resolve<DataPoolService>().ResetNullStrategy();
        }
        return results;
    }

    private static TtVariantResult RunVariant(
        BenchmarkResult line,
        TtVariant variant,
        IProgress<string> status,
        IProgress<TtMoveRecord> moves,
        CancellationToken cancellationToken)
    {
        var session = line.Session;

        TranspositionTableService.CapacityOverrideMB = variant.MainMB;
        NullStrategy.CapacityOverrideMB = variant.NullMB;

        // Free tables of the previous variant and rebuild the shared null strategy for the new position / size.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        ContainerLocator.Current.Resolve<DataPoolService>().ResetNullStrategy();

        var moveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
        var position = new Position();
        var factory = ContainerLocator.Current.Resolve<IStrategyFactory>();
        var strategy = factory.GetStrategy(session.Depth, position, session.Strategy);

        TtStatistics.Reset();

        var result = new TtVariantResult { Variant = variant };
        double searchMs = 0;
        int ply = 0;
        var lastTables = new List<TtTableStats>();

        try
        {
            foreach (var saved in line.Moves)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ply++;
                var record = new TtMoveRecord
                {
                    VariantName = variant.Name,
                    MoveNumber = ply,
                    IsEngineMove = saved.IsEngineMove,
                    UciMove = saved.UciMove,
                    MoveNotation = saved.MoveNotation
                };

                if (saved.IsEngineMove)
                {
                    long nodes0 = TtStatistics.Nodes, qNodes0 = TtStatistics.QNodes;
                    var sw = Stopwatch.StartNew();
                    var search = strategy.GetResult();
                    sw.Stop();
                    record.Nodes = TtStatistics.Nodes - nodes0;
                    record.QNodes = TtStatistics.QNodes - qNodes0;
                    result.Nodes += record.Nodes;
                    result.QNodes += record.QNodes;
                    searchMs += sw.Elapsed.TotalMilliseconds;

                    result.EngineMoves++;
                    record.MatchesBaseline = search.Move?.Key == saved.MoveKey;
                    if (record.MatchesBaseline) result.MatchingMoves++;
                    record.DurationMs = sw.Elapsed.TotalMilliseconds;
                    lastTables = TtStatistics.Capture();

                    if (variant.AdvanceMain) strategy.ExecuteAsyncAction();
                    if (variant.AdvanceNull) strategy.ExecuteNullAsyncAction();
                }

                record.Tables = lastTables;
                record.ProcessMemoryMB = Process.GetCurrentProcess().WorkingSet64 / 1_048_576.0;
                record.ManagedMemoryMB = GC.GetTotalMemory(false) / 1_048_576.0;
                if (saved.IsEngineMove)
                {
                    record.Report = TtReportBuilder.BuildTablesReport(record.Tables);
                    record.Advice = TtReportBuilder.BuildAdvice($"{variant.Name} ply {ply}", record.Tables);
                }

                result.Moves.Add(record);
                moves?.Report(record);
                status?.Report($"{variant.Name}: ply {ply}/{line.Moves.Count}, engine move {result.EngineMoves}");

                var move = moveProvider.Get(saved.MoveKey);
                if (move == null) break;
                ApplyMove(position, move);
            }
        }
        finally
        {
            // MoveHistoryService is a process-wide singleton: leave it empty for the next variant.
            position.Clear();
        }

        result.TotalMs = searchMs;
        result.Tables = TtStatistics.Capture();
        result.Report = TtReportBuilder.BuildReport(result);
        result.Advice = TtReportBuilder.BuildAdvice(result);
        return result;
    }

    private static void ApplyMove(Position position, MoveBase move)
    {
        if (position.GetHistory().Any())
            position.Make(move);
        else
            position.MakeFirst(move);
    }
}
