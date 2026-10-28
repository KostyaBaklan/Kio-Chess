using StockfishBenchmark.Models;

namespace StockfishBenchmark.Services;

public interface ITtBenchmarkRunner
{
    /// <summary>
    /// Replays the recorded game (both sides) once per variant, searching at every engine ply,
    /// so every variant sees exactly the same position sequence.
    /// </summary>
    Task<List<TtVariantResult>> RunAsync(
        BenchmarkResult line,
        IReadOnlyList<TtVariant> variants,
        IProgress<string> status = null,
        IProgress<TtVariantResult> completed = null,
        IProgress<TtMoveRecord> moves = null,
        CancellationToken cancellationToken = default);
}
