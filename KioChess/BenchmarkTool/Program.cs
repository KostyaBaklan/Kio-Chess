using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using EngineBenchmark;

internal class Program
{

    private static void Main(string[] args)
    {
        // Custom config with CSV and Markdown exporters
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddExporter(CsvExporter.Default)
            .AddExporter(MarkdownExporter.Default);

        BenchmarkHelper.InitializeLogger();

        try
        {
            var summary = BenchmarkRunner.Run<MoveHistorySorting>(config);
        }
        catch (Exception ex)
        {
            BenchmarkHelper.LogError($"Benchmark failed: {ex.Message}");
            throw;
        }
        finally
        {
            BenchmarkHelper.CloseLogger();
        }
        Console.WriteLine("Hello, World!");
    }
}