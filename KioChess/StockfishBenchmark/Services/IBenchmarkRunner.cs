using StockfishBenchmark.Models;

namespace StockfishBenchmark.Services;

public interface IBenchmarkRunner
{
    /// <summary>
    /// Runs a fresh benchmark game against Stockfish and returns the result.
    /// </summary>
    /// <param name="session">Parameters for this run.</param>
    /// <param name="progress">Optional per-move progress callback.</param>
    /// <param name="cancellationToken">Cancellation support.</param>
    Task<BenchmarkResult> RunAsync(
        BenchmarkSession session,
        IProgress<BenchmarkMoveRecord> progress = null,
        CancellationToken cancellationToken = default);
}
