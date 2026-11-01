using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace TexelTool;

internal static partial class Program
{
    private static readonly int[] Directions = [1, -1];

    private static int[] GetValues(List<Parameter> parameters)
    {
        var values = new int[parameters.Count];
        for (int i = 0; i < values.Length; i++) values[i] = parameters[i].Get();
        return values;
    }

    private static void ApplyAll(List<Parameter> parameters, int[] values)
    {
        for (int i = 0; i < values.Length; i++) parameters[i].Set(values[i]);
        for (byte phase = 0; phase < 3; phase++) _factory.GetEvaluationService(phase).Reload(_configuration, _staticValueProvider);
        _board.ResetEvaluationCaches();
    }

    private static bool TryImprove(Parameter p, int step, TexelDataSet data, double k, double[] phaseError)
    {
        if (data.PhaseIndices[p.Phase].Length == 0) return false;

        int original = p.Get();
        foreach (var direction in Directions)
        {
            int candidate = original + direction * step;
            if (candidate < p.Min || candidate > p.Max) continue;

            Apply(p, candidate);
            double error = PhaseError(data, p.Phase, k);

            if (error < phaseError[p.Phase])
            {
                phaseError[p.Phase] = error;
                return true;
            }
        }

        Apply(p, original);
        return false;
    }

    private static void WriteInts(string path, int[] values) =>
        File.WriteAllBytes(path, MemoryMarshal.AsBytes(values.AsSpan()).ToArray());

    private static int[] ReadInts(string path) =>
        MemoryMarshal.Cast<byte, int>(File.ReadAllBytes(path)).ToArray();

    #region Worker

    private static void RunWorker(int index, int count, string datasetPath)
    {
        var data = TexelDataSet.Load(datasetPath, out var k);
        var parameters = BuildParameters();

        Console.WriteLine("READY");

        string line;
        while ((line = Console.ReadLine()) != null)
        {
            var parts = line.Split('\t');
            if (parts[0] == "EXIT") break;
            if (parts[0] != "ROUND") continue;

            int step = int.Parse(parts[1]);
            ApplyAll(parameters, ReadInts(parts[2]));

            var phaseError = new double[3];
            for (int phase = 0; phase < 3; phase++) phaseError[phase] = PhaseError(data, phase, k);
            double start = phaseError.Sum() / data.Count;

            int improved = 0;
            if (step > 0)
            {
                for (int i = index; i < parameters.Count; i += count)
                {
                    if (TryImprove(parameters[i], step, data, k, phaseError)) improved++;
                }
            }

            WriteInts(parts[3], GetValues(parameters));
            Console.WriteLine($"DONE\t{start.ToString("R", CultureInfo.InvariantCulture)}\t{improved}");
        }
    }

    #endregion

    #region Orchestrator

    private static string ReadReply(Process worker, string tag)
    {
        string line;
        while ((line = worker.StandardOutput.ReadLine()) != null)
        {
            if (line.StartsWith(tag)) return line;
        }
        throw new InvalidOperationException("Worker process exited unexpectedly.");
    }

    private static void TuneParallel(List<Parameter> parameters, string datasetPath)
    {
        int n = _cfg.WorkerCount;
        string workDir = Path.GetFullPath(Path.Combine(_cfg.OutputDirectory, "work"));
        Directory.CreateDirectory(workDir);
        string valuesFile = Path.Combine(workDir, "values.bin");

        var processPath = Environment.ProcessPath;
        var prefix = Path.GetFileNameWithoutExtension(processPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase)
            ? Environment.GetCommandLineArgs()[0]
            : null;

        var workers = new List<Process>();
        var watch = Stopwatch.StartNew();

        try
        {
            for (int i = 0; i < n; i++)
            {
                var info = new ProcessStartInfo(processPath)
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };
                if (prefix != null) info.ArgumentList.Add(prefix);
                info.ArgumentList.Add("--worker");
                info.ArgumentList.Add(i.ToString());
                info.ArgumentList.Add(n.ToString());
                info.ArgumentList.Add(datasetPath);
                workers.Add(Process.Start(info));
            }

            foreach (var worker in workers) ReadReply(worker, "READY");
            Console.WriteLine($"[Parallel] {n} workers ready, {watch.Elapsed:hh\\:mm\\:ss}");

            (double error, int improved, int[] merged) Round(int step, int[] values)
            {
                WriteInts(valuesFile, values);

                for (int i = 0; i < n; i++)
                {
                    workers[i].StandardInput.WriteLine($"ROUND\t{step}\t{valuesFile}\t{Path.Combine(workDir, $"result{i}.bin")}");
                }

                var merged = (int[])values.Clone();
                double error = 0;
                int improved = 0;

                for (int i = 0; i < n; i++)
                {
                    var reply = ReadReply(workers[i], "DONE").Split('\t');
                    if (i == 0) error = double.Parse(reply[1], CultureInfo.InvariantCulture);
                    improved += int.Parse(reply[2]);

                    var result = ReadInts(Path.Combine(workDir, $"result{i}.bin"));
                    for (int p = i; p < merged.Length; p += n) merged[p] = result[p];
                }

                return (error, improved, merged);
            }

            var values = GetValues(parameters);
            var best = (int[])values.Clone();
            double bestError = double.MaxValue;

            foreach (var step in _cfg.StepSizes)
            {
                for (int epoch = 0; epoch < _cfg.MaxEpochsPerStep; epoch++)
                {
                    var (error, improved, merged) = Round(step, values);

                    if (error > bestError)
                    {
                        values = (int[])best.Clone();
                        Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}: merge raised error {bestError:F6} -> {error:F6}, reverted, next step, {watch.Elapsed:hh\\:mm\\:ss}");
                        break;
                    }

                    best = (int[])values.Clone();
                    bestError = error;
                    Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep}: error {error:F6}, improved {improved}, {watch.Elapsed:hh\\:mm\\:ss}");

                    if (improved == 0) break;
                    values = merged;
                }
            }

            var final = Round(0, values);
            if (final.error > bestError) values = best;
            Console.WriteLine($"[Parallel] done, final error {Math.Min(final.error, bestError):F6}, {watch.Elapsed:hh\\:mm\\:ss}");

            ApplyAll(parameters, values);
        }
        finally
        {
            foreach (var worker in workers)
            {
                try
                {
                    worker.StandardInput.WriteLine("EXIT");
                    worker.WaitForExit(10000);
                    if (!worker.HasExited) worker.Kill();
                }
                catch (Exception)
                {
                }
                worker.Dispose();
            }
        }
    }

    #endregion
}
