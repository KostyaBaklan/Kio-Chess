using System.IO;
using Newtonsoft.Json;
using StockfishBenchmark.Models;

namespace StockfishBenchmark.Services;

public class BenchmarkFileService : IBenchmarkFileService
{
    private const string Folder = "Benchmarks";

    public string GetDefaultPath(BenchmarkSession s)
    {
        Directory.CreateDirectory(Folder);
        var name = $"{s.Strategy}_d{s.Depth}_{s.Color}_{s.RunDate:yyyyMMdd_HHmmss}.json";
        return Path.Combine(Folder, name);
    }

    public void Save(BenchmarkResult result, string filePath = null)
    {
        filePath ??= GetDefaultPath(result.Session);
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(filePath, JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    public BenchmarkResult Load(string filePath)
        => JsonConvert.DeserializeObject<BenchmarkResult>(File.ReadAllText(filePath));
}
