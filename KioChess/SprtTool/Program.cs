using System.Diagnostics;
using System.Text.Json;
using SprtTool;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--player") { PlayerMode.Run(); return 0; }
        return Menu.Run();
    }
}

// Double-click SprtTool.exe and type a number. Paths come only from Config\SprtConfig.json.
internal static class Menu
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "SprtConfig.json");
    private static int _lastExitCode;

    public static int Run()
    {
        // The engine loads Config\Configuration.json relative to the current folder.
        Environment.CurrentDirectory = AppContext.BaseDirectory;
        while (true)
        {
            if (!Console.IsOutputRedirected) Console.Clear();
            SprtConfig c;
            SprtPaths p;
            try
            {
                c = Load();
                p = new SprtPaths(c);
                p.CreateFolders();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot load {ConfigPath}");
                Console.WriteLine(e.Message);
                Pause();
                return 1;
            }

            ShowStatus(c, p);
            Console.WriteLine();
            Console.WriteLine("  1  Build the openings file from the PGN files");
            Console.WriteLine("  2  Run the SPRT test");
            Console.WriteLine("  3  Open the working folder");
            Console.WriteLine("  4  Show the first openings in readable notation");
            Console.WriteLine("  5  Deploy the current build to baseline or candidate");
            Console.WriteLine("  0  Exit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    Safely(() => BuildOpenings(c, p));
                    Pause();
                    break;
                case "2":
                    Safely(() => RunTest(c, p));
                    Pause();
                    break;
                case "3":
                    Safely(() => Process.Start(new ProcessStartInfo(p.WorkingDirectory) { UseShellExecute = true }));
                    break;
                case "4":
                    Safely(() => ShowOpenings(p));
                    Pause();
                    break;
                case "5":
                    Safely(() => DeployBuild(p));
                    Pause();
                    break;
                case "0":
                case null:
                    return _lastExitCode;
            }
        }
    }

    private static SprtConfig Load() =>
        JsonSerializer.Deserialize<SprtConfig>(File.ReadAllText(ConfigPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip })
        ?? throw new InvalidOperationException("SprtConfig.json is empty.");

    private static void ShowStatus(SprtConfig c, SprtPaths p)
    {
        Console.WriteLine("SPRT test tool");
        Console.WriteLine();
        Console.WriteLine($"Working folder : {p.WorkingDirectory}");
        Console.WriteLine($"Baseline       : {EngineStatus(p.BaselineDir, c.ExeName, p)}");
        Console.WriteLine($"Candidate      : {EngineStatus(p.CandidateDir, c.ExeName, p)}");
        Console.WriteLine($"PGN files      : {PgnFiles(p).Length} in {Rel(p.PgnDir, p)}");
        Console.WriteLine($"Openings file  : {OpeningsStatus(p)}");
    }

    private static string EngineStatus(string dir, string exe, SprtPaths p) =>
        File.Exists(Path.Combine(dir, exe))
            ? $"ready ({Rel(dir, p)})"
            : $"MISSING - copy the SprtTool build folder into {Rel(dir, p)}";

    private static string OpeningsStatus(SprtPaths p) =>
        File.Exists(p.OpeningsFile)
            ? $"{CountOpenings(p.OpeningsFile)} openings in {Rel(p.OpeningsFile, p)}"
            : "not built yet (option 1)";

    private static void BuildOpenings(SprtConfig c, SprtPaths p)
    {
        var pgn = PgnFiles(p);
        if (pgn.Length == 0)
        {
            Console.WriteLine($"No .pgn or .zst files in {Rel(p.PgnDir, p)}. Copy your games there and try again.");
            return;
        }
        if (File.Exists(p.OpeningsFile) && !Confirm("The openings file already exists. Replace it?")) return;

        if (c.OpeningPlies < 1)
        {
            Console.WriteLine("OpeningPlies in SprtConfig.json must be at least 1.");
            return;
        }

        if (!File.Exists(p.GamesDbFile))
        {
            Console.WriteLine($"games.db not found at {p.GamesDbFile}. Set GamesDbFile in SprtConfig.json.");
            return;
        }

        Console.WriteLine($"Reading {pgn.Length} file(s). Every game with both ratings >= {c.OpeningMinElo} is used; all distinct {c.OpeningPlies}-ply openings are kept. This can take a long time.");
        int written = OpeningBuilder.Build(p.PgnDir, p.OpeningsFile, p.GamesDbFile, c.OpeningPlies, c.OpeningMinElo, c.OpeningMaxAbsEval);
        if (written < c.OpeningCount)
            Console.WriteLine($"Only {written} openings were found, below the target of {c.OpeningCount}. Lower OpeningMinElo or raise OpeningMaxAbsEval in SprtConfig.json, then rebuild.");
    }

    private static void ShowOpenings(SprtPaths p)
    {
        if (!File.Exists(p.OpeningsFile))
        {
            Console.WriteLine("No openings file yet. Run option 1 first.");
            return;
        }
        Console.Write("How many rows to show? ");
        if (!int.TryParse(Console.ReadLine()?.Trim(), out var rows) || rows < 1)
        {
            Console.WriteLine("Enter a whole number, 1 or more.");
            return;
        }
        OpeningNotation.Print(p.OpeningsFile, rows);
    }

    private static void DeployBuild(SprtPaths p)
    {
        Console.WriteLine();
        Console.WriteLine("  1  Deploy to baseline");
        Console.WriteLine("  2  Deploy to candidate");
        Console.WriteLine("  0  Cancel");
        Console.Write("Choose a target: ");
        switch (Console.ReadLine()?.Trim())
        {
            case "1": CopyBuild(p, p.BaselineDir); break;
            case "2": CopyBuild(p, p.CandidateDir); break;
        }
    }

    // Copies the build folder to an engine folder, skipping the Sprt working folder and .pdb files.
    private static void CopyBuild(SprtPaths p, string target)
    {
        if (!Confirm($"Copy the current build into {Rel(target, p)} and overwrite existing files?")) return;

        var source = AppContext.BaseDirectory;
        var sprt = Path.GetFullPath(p.WorkingDirectory) + Path.DirectorySeparatorChar;
        int copied = 0, failed = 0;
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            if (Path.GetFullPath(file).StartsWith(sprt, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(Path.GetExtension(file), ".pdb", StringComparison.OrdinalIgnoreCase)) continue;

            var dest = Path.Combine(target, Path.GetRelativePath(source, file));
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest, true);
                copied++;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                failed++;
                Console.WriteLine($"Could not copy {Path.GetRelativePath(source, file)}: {e.Message}");
            }
        }
        string note = failed > 0 ? $", {failed} failed (close any running engine and try again)." : ".";
        Console.WriteLine($"Copied {copied} file(s) to {Rel(target, p)}{note}");
    }

    // The engine loads these three files from its own Config folder.
    private static readonly string[] EngineConfigFiles = { "Configuration.json", "EvaluationConfiguration.json", "StaticTables.json" };

    private static void RunTest(SprtConfig c, SprtPaths p)
    {
        var problems = SetupProblems(c, p);
        if (!File.Exists(p.OpeningsFile) || CountOpenings(p.OpeningsFile) == 0) problems.Add("openings file (menu option 1)");
        if (problems.Count == 0 && SameBuild(c, p))
            problems.Add("baseline and candidate are identical (same engine and Config files), so the test would measure nothing. Change the candidate first.");
        if (problems.Count > 0)
        {
            Console.WriteLine("Cannot start the test:");
            foreach (var m in problems) Console.WriteLine($"  - {m}");
            return;
        }

        Console.WriteLine("Press Ctrl+C to stop after the games in progress.");
        Console.WriteLine($"Log file: {p.LogFile}");
        _lastExitCode = Referee.Run(c, p);
    }

    private static string[] PgnFiles(SprtPaths p) =>
        Directory.GetFiles(p.PgnDir, "*.pgn").Concat(Directory.GetFiles(p.PgnDir, "*.zst")).ToArray();

    private static int CountOpenings(string file) => File.ReadLines(file).Count(l => l.Trim().Length > 0);

    // Each engine folder needs the exe and the Config files the engine reads from its working folder.
    private static List<string> SetupProblems(SprtConfig c, SprtPaths p)
    {
        var problems = new List<string>();
        foreach (var (name, dir) in new[] { ("baseline", p.BaselineDir), ("candidate", p.CandidateDir) })
        {
            if (!File.Exists(Path.Combine(dir, c.ExeName))) problems.Add($"{name} engine {c.ExeName} in {Rel(dir, p)}");
            foreach (var file in EngineConfigFiles)
                if (!File.Exists(Path.Combine(dir, "Config", file))) problems.Add($"{name} Config\\{file} in {Rel(dir, p)}");
        }
        return problems;
    }

    // True when the exe and every Config file are byte-identical in both folders. Call only after SetupProblems is empty.
    private static bool SameBuild(SprtConfig c, SprtPaths p)
    {
        var files = new[] { c.ExeName }.Concat(EngineConfigFiles.Select(f => Path.Combine("Config", f)));
        return files.All(f => File.ReadAllBytes(Path.Combine(p.BaselineDir, f))
            .AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(p.CandidateDir, f))));
    }

    private static string Rel(string path, SprtPaths p) => Path.GetRelativePath(p.WorkingDirectory, path);

    private static bool Confirm(string question)
    {
        Console.Write($"{question} (y/n): ");
        return string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press Enter to return to the menu...");
        Console.ReadLine();
    }

    private static void Safely(Action action)
    {
        try { action(); }
        catch (Exception e) { Console.WriteLine($"ERROR: {e.Message}"); }
    }
}