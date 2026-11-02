using System.Collections.Concurrent;
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

            if (TryCandidate(p, candidate, data, k, phaseError)) return true;
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
                int total = (parameters.Count - index + count - 1) / count;
                int done = 0;
                var watch = Stopwatch.StartNew();
                long lastReport = 0;

                for (int i = index; i < parameters.Count; i += count)
                {
                    if (TryImprove(parameters[i], step, data, k, phaseError)) improved++;
                    done++;

                    if (watch.ElapsedMilliseconds - lastReport >= _cfg.ProgressIntervalSeconds * 1000L)
                    {
                        lastReport = watch.ElapsedMilliseconds;
                        Console.WriteLine($"PROGRESS\t{index}\t{done}\t{total}\t{improved}");
                    }
                }
            }

            WriteInts(parts[3], GetValues(parameters));
            Console.WriteLine($"DONE\t{start.ToString("R", CultureInfo.InvariantCulture)}\t{improved}");
        }
    }

    #endregion

    #region Orchestrator

    private const string EofMarker = "\0EOF";

    private sealed class WorkerLink
    {
        public Process Process;
        public readonly BlockingCollection<string> Replies = new();
        public int Done;
        public int Total;
        public int Improved;
    }

    private static string ReadReply(WorkerLink link, string tag)
    {
        while (true)
        {
            var line = link.Replies.Take();
            if (line == EofMarker) throw new InvalidOperationException("Worker process exited unexpectedly.");
            if (line.StartsWith(tag)) return line;
        }
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

        var workers = new List<WorkerLink>();
        var watch = Stopwatch.StartNew();
        string roundLabel = "";
        long lastPrint = 0;

        void PrintProgress()
        {
            lock (workers)
            {
                if (watch.ElapsedMilliseconds - lastPrint < _cfg.ProgressIntervalSeconds * 1000L) return;
                lastPrint = watch.ElapsedMilliseconds;

                int done = 0, total = 0, improved = 0;
                double min = 100, max = 0;
                foreach (var w in workers)
                {
                    done += w.Done;
                    total += w.Total;
                    improved += w.Improved;
                    double percent = w.Total == 0 ? 100 : 100.0 * w.Done / w.Total;
                    min = Math.Min(min, percent);
                    max = Math.Max(max, percent);
                }

                Console.WriteLine($"[Tune] {roundLabel}: {100.0 * done / Math.Max(total, 1):F1}% ({done}/{total}) improved {improved}, workers {min:F0}%..{max:F0}%, {watch.Elapsed:hh\\:mm\\:ss}");
            }
        }

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
                var link = new WorkerLink { Process = Process.Start(info) };
                link.Process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data == null)
                    {
                        link.Replies.Add(EofMarker);
                        return;
                    }

                    if (e.Data.StartsWith("PROGRESS\t"))
                    {
                        var fields = e.Data.Split('\t');
                        link.Done = int.Parse(fields[2]);
                        link.Improved = int.Parse(fields[4]);
                        PrintProgress();
                    }
                    else
                    {
                        link.Replies.Add(e.Data);
                    }
                };
                link.Process.BeginOutputReadLine();
                workers.Add(link);
            }

            foreach (var worker in workers) ReadReply(worker, "READY");
            Console.WriteLine($"[Parallel] {n} workers ready, {watch.Elapsed:hh\\:mm\\:ss}");

            (double error, int improved, int[][] results) Round(int step, int[] values)
            {
                WriteInts(valuesFile, values);

                lock (workers)
                {
                    lastPrint = watch.ElapsedMilliseconds;
                    for (int i = 0; i < n; i++)
                    {
                        workers[i].Done = 0;
                        workers[i].Improved = 0;
                        workers[i].Total = (parameters.Count - i + n - 1) / n;
                    }
                }

                for (int i = 0; i < n; i++)
                {
                    workers[i].Process.StandardInput.WriteLine($"ROUND\t{step}\t{valuesFile}\t{Path.Combine(workDir, $"result{i}.bin")}");
                }

                var results = new int[n][];
                double error = 0;
                int improved = 0;

                for (int i = 0; i < n; i++)
                {
                    var reply = ReadReply(workers[i], "DONE").Split('\t');
                    if (i == 0) error = double.Parse(reply[1], CultureInfo.InvariantCulture);
                    improved += int.Parse(reply[2]);
                    results[i] = ReadInts(Path.Combine(workDir, $"result{i}.bin"));
                }

                return (error, improved, results);
            }

            var weightIndices = new List<int>();
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Name.StartsWith("KingSafety.AttackWeight[")) weightIndices.Add(i);
            }

            int[] Merge(int[] values, int[][] results, int count)
            {
                var merged = (int[])values.Clone();
                for (int i = 0; i < count; i++)
                {
                    for (int p = i; p < merged.Length; p += n) merged[p] = results[i][p];
                }

                for (int j = 1; j < weightIndices.Count; j++)
                {
                    if (merged[weightIndices[j]] < merged[weightIndices[j - 1]]) merged[weightIndices[j]] = merged[weightIndices[j - 1]];
                }
                return merged;
            }

            double[] EvaluateAll(int[][] candidates)
            {
                for (int i = 0; i < n; i++)
                {
                    string candidateFile = Path.Combine(workDir, $"candidate{i}.bin");
                    WriteInts(candidateFile, candidates[i]);
                    workers[i].Process.StandardInput.WriteLine($"ROUND\t0\t{candidateFile}\t{Path.Combine(workDir, $"eval{i}.bin")}");
                }

                var errors = new double[n];
                for (int i = 0; i < n; i++)
                {
                    errors[i] = double.Parse(ReadReply(workers[i], "DONE").Split('\t')[1], CultureInfo.InvariantCulture);
                }
                return errors;
            }

            var values = GetValues(parameters);
            var best = (int[])values.Clone();
            double bestError = double.MaxValue;

            foreach (var step in _cfg.StepSizes)
            {
                _stepIndex = Array.IndexOf(_cfg.StepSizes, step);
                if (_stepIndex < _resumeStepIndex) continue;
                for (int epoch = 0; epoch < _cfg.MaxEpochsPerStep; epoch++)
                {
                    roundLabel = $"step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep}";
                    var (error, improved, results) = Round(step, values);
                    bestError = error;
                    Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep} workers finished: start error {error:F6}, improved {improved}, {watch.Elapsed:hh\\:mm\\:ss}");

                    if (improved == 0) break;

                    var candidates = new int[n][];
                    for (int j = 0; j < n; j++) candidates[j] = Merge(values, results, j + 1);
                    var errors = EvaluateAll(candidates);

                    int bestIndex = -1;
                    double mergedError = error;
                    for (int j = 0; j < n; j++)
                    {
                        if (errors[j] < mergedError)
                        {
                            mergedError = errors[j];
                            bestIndex = j;
                        }
                    }

                    if (bestIndex < 0)
                    {
                        Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}: no merge lowers error, next step, {watch.Elapsed:hh\\:mm\\:ss}");
                        break;
                    }

                    values = candidates[bestIndex];
                    best = (int[])values.Clone();
                    bestError = mergedError;
                    ApplyAll(parameters, values);
                    Save(parameters);
                    Console.WriteLine($"[Tune] step {step}, epoch {epoch + 1}/{_cfg.MaxEpochsPerStep} merged workers 0..{bestIndex} of {n - 1}: error {error:F6} -> {mergedError:F6}, {watch.Elapsed:hh\\:mm\\:ss}");
                    if (error - mergedError < _cfg.MinEpochImprovement) break;
                }

                ApplyAll(parameters, values);
                _stepIndex++;
                Save(parameters);
            }

            Console.WriteLine($"[Parallel] done, final error {bestError:F6}, {watch.Elapsed:hh\\:mm\\:ss}");

            ApplyAll(parameters, values);
        }
        finally
        {
            foreach (var link in workers)
            {
                var worker = link.Process;
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

            try
            {
                if (Directory.Exists(workDir)) Directory.Delete(workDir, true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Parallel] could not delete {workDir}: {e.Message}");
            }
        }
    }

    #endregion
}
