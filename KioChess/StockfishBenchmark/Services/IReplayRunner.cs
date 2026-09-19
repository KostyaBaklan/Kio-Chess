using StockfishBenchmark.Models;

namespace StockfishBenchmark.Services;

public interface IReplayRunner
{
    /// <summary>
    /// Replays a saved baseline result and produces a comparison table.
    /// Engine plies are re-run with the current engine; Stockfish plies are taken as-is.
    /// </summary>
    Task<List<ComparisonRow>> CompareAsync(
        BenchmarkResult baseline,
        IProgress<ComparisonRow> progress = null,
        CancellationToken cancellationToken = default);
}
