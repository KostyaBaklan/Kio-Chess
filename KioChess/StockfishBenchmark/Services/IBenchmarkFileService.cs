using StockfishBenchmark.Models;

namespace StockfishBenchmark.Services;

public interface IBenchmarkFileService
{
    // Default path: Benchmarks\{strategy}_d{depth}_{color}_{yyyyMMdd_HHmmss}.json
    string GetDefaultPath(BenchmarkSession session);
    void Save(BenchmarkResult result, string filePath = null);
    BenchmarkResult Load(string filePath);
}
