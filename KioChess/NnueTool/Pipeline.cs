using Engine.Models.Config;

namespace NnueTool;

/// <summary>
/// One-click resumable pipeline. For every *.pgn in Data.SourceDirectory:
/// filter -> delete source PGN -> label (child processes) -> merge into train.bin / val.bin -> delete work files.
/// State lives in OutputDirectory\work\&lt;pgn name&gt;\; rerunning continues from where the previous run stopped.
/// </summary>
internal static class Pipeline
{
    private const string FilteredName = "filtered.pgn";

    public static int Run(NnueDataConfiguration data, int workers, Func<string, int, string, int> runWorkers, string logPath)
    {
        var workRoot = Path.Combine(data.OutputDirectory, "work");
        Directory.CreateDirectory(workRoot);

        // A chunk merged but not yet trained (crash during training) is trained first.
        if (File.Exists(TrainPath(data)) && Train(data) != 0)
            return 3;

        // Interrupted sources first (their PGN may already be deleted), then new PGNs.
        var names = Directory.GetDirectories(workRoot).Select(Path.GetFileName)
            .Concat(Directory.GetFiles(data.SourceDirectory, "*.pgn").Select(Path.GetFileNameWithoutExtension))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
        {
            Console.WriteLine($"Nothing to do: no *.pgn in {data.SourceDirectory} and no pending work in {workRoot}");
            return 0;
        }

        foreach (var name in names)
        {
            var source = Path.Combine(data.SourceDirectory, name + ".pgn");
            var workDir = Path.Combine(workRoot, name);
            Directory.CreateDirectory(workDir);
            Console.WriteLine($"=== {name} ===");

            if (!Filter(source, workDir, data.Filter))
                return 1;

            var status = Label(workDir, workers, runWorkers);
            if (status != 0)
                return status;

            Merge(workDir, data, logPath, name);

            if (Train(data) != 0)
                return 3;
        }

        Console.WriteLine($"Pipeline finished; nets in configured Training.OutputDirectory, validation set {Path.Combine(data.OutputDirectory, "val.bin")}");
        return 0;
    }

    private static bool Filter(string source, string workDir, NnueFilterConfiguration filter)
    {
        var filtered = Path.Combine(workDir, FilteredName);

        if (!File.Exists(filtered))
        {
            if (!File.Exists(source))
            {
                Console.WriteLine($"Source {source} is missing and {filtered} was not created; remove {workDir} or restore the PGN.");
                return false;
            }

            var temp = filtered + ".tmp";
            FilterCommand.Run(source, temp, filter);
            File.Move(temp, filtered, true);
        }

        if (File.Exists(source))
        {
            File.Delete(source);
            Console.WriteLine($"Deleted source {source}");
        }

        return true;
    }

    private static int Label(string workDir, int workers, Func<string, int, string, int> runWorkers)
    {
        var workersFile = Path.Combine(workDir, "labeled.workers");
        if (File.Exists(workersFile))
            workers = int.Parse(File.ReadAllText(workersFile)); // partition is fixed once labeling started

        if (CountDone(workDir) < workers)
        {
            runWorkers(Path.Combine(workDir, FilteredName), workers, workDir);
        }

        int done = CountDone(workDir);
        if (done < workers)
        {
            Console.WriteLine($"Labeling stopped with {done}/{workers} workers finished (time limit, Ctrl+C or failure). Run NnueTool again to continue.");
            return 2;
        }

        return 0;
    }

    private static void Merge(string workDir, NnueDataConfiguration data, string logPath, string source)
    {
        var output = data.OutputDirectory;
        var inputs = Directory.GetFiles(workDir, "labeled-*.bin").OrderBy(f => f).ToArray();

        // Only the new chunk goes to train.bin; val.bin is created once (first chunk) and then kept fixed.
        MergeCommand.Run(inputs, output, data.Merge, data.Label.MaxAbsScore, keepValidation: true, logPath: logPath, source: source);

        Directory.Delete(workDir, true);
        Console.WriteLine($"Deleted work files {workDir}");
    }

    private static string TrainPath(NnueDataConfiguration data) => Path.Combine(data.OutputDirectory, "train.bin");

    /// <summary>Runs NnueTrainer (resumes from the latest checkpoint) on train.bin; deletes train.bin on success.</summary>
    private static int Train(NnueDataConfiguration data)
    {
        var trainer = Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "NnueTrainer.exe" : "NnueTrainer");
        if (!File.Exists(trainer))
        {
            Console.WriteLine($"Trainer not found: {trainer}");
            return 1;
        }

        Console.WriteLine("=== Training ===");
        using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(trainer, "--chunk")
        {
            UseShellExecute = false,
            WorkingDirectory = AppContext.BaseDirectory
        });
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Training failed (exit {process.ExitCode}); train.bin kept, run NnueTool again to retry.");
            return process.ExitCode;
        }

        File.Delete(TrainPath(data));
        Console.WriteLine("Deleted train.bin");
        return 0;
    }

    private static int CountDone(string workDir) => Directory.GetFiles(workDir, "labeled-*.done").Length;
}
