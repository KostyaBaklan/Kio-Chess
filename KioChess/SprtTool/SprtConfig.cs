using System.Globalization;

namespace SprtTool;

// All paths are relative to <folder of SprtTool.exe>\<WorkingDirectory>, see SprtPaths.
internal sealed class SprtConfig
{
    public string WorkingDirectory { get; set; } = "Sprt";
    public string BaselineDir { get; set; } = "baseline";
    public string CandidateDir { get; set; } = "candidate";
    public string ExeName { get; set; } = "SprtTool.exe";
    public string PgnDir { get; set; } = "pgn";
    public string OpeningsDir { get; set; } = "openings";   // file name is openings_<OpeningMinElo>_<OpeningPlies>.txt
    public string LogDir { get; set; } = "logs";   // file name is built from Elo0, Elo1, Depth, MaxPairs and MaxPlies
    public string GamesDbFile { get; set; } = "C:/Dev/ChessDB/games.db";   // read-only; popularity source for the openings sort
    public int OpeningCount { get; set; } = 100000;   // target; the build keeps every distinct opening and warns if it finds fewer
    public int OpeningPlies { get; set; } = 6;         // fixed length of every opening, in plies
    public int OpeningMinElo { get; set; } = 2600;
    public int OpeningMaxAbsEval { get; set; } = 120;
    public int OpeningShuffleSeed { get; set; } = 12345;   // orders the openings pool; same seed, same order
    public int Depth { get; set; } = 6;
    public int Concurrency { get; set; } = 4;
    public double Elo0 { get; set; } = 0;
    public double Elo1 { get; set; } = 5;
    public double Alpha { get; set; } = 0.05;
    public double Beta { get; set; } = 0.05;
    public int MaxPairs { get; set; } = 10000;
    public int MaxPlies { get; set; } = 300;
    public int AdjudicateScore { get; set; } = 1000;   // centipawns, side-to-move score; 0 disables
    public int AdjudicatePlies { get; set; } = 6;      // consecutive plies above the score; 0 disables
    public int ReplyTimeoutSeconds { get; set; } = 300;
}

// Resolves every configured path. Paths must be relative, so nothing depends on the current directory.
internal sealed class SprtPaths
{
    public string WorkingDirectory { get; }
    public string BaselineDir { get; }
    public string CandidateDir { get; }
    public string PgnDir { get; }
    public string OpeningsFile { get; }
    public string LogFile { get; }
    public string GamesDbFile { get; }

    public SprtPaths(SprtConfig c)
    {
        WorkingDirectory = Resolve(AppContext.BaseDirectory, c.WorkingDirectory, nameof(c.WorkingDirectory));
        BaselineDir = Resolve(WorkingDirectory, c.BaselineDir, nameof(c.BaselineDir));
        CandidateDir = Resolve(WorkingDirectory, c.CandidateDir, nameof(c.CandidateDir));
        PgnDir = Resolve(WorkingDirectory, c.PgnDir, nameof(c.PgnDir));
        OpeningsFile = Path.Combine(Resolve(WorkingDirectory, c.OpeningsDir, nameof(c.OpeningsDir)),
                                    $"openings_{c.OpeningMinElo}_{c.OpeningPlies}.txt");
        LogFile = Path.Combine(Resolve(WorkingDirectory, c.LogDir, nameof(c.LogDir)),
                               $"sprt_elo0-{Tag(c.Elo0)}_elo1-{Tag(c.Elo1)}_depth{c.Depth}_pairs{c.MaxPairs}_plies{c.MaxPlies}.log");
        GamesDbFile = ResolveStatic(c.GamesDbFile, nameof(c.GamesDbFile));
    }

    public void CreateFolders()
    {
        foreach (var dir in new[] { WorkingDirectory, BaselineDir, CandidateDir, PgnDir,
                     Path.GetDirectoryName(OpeningsFile), Path.GetDirectoryName(LogFile) })
            Directory.CreateDirectory(dir);
    }

    // 2.5 becomes "2_5" so the file name has no dot.
    private static string Tag(double value) => value.ToString(CultureInfo.InvariantCulture).Replace('.', '_');

    private static string Resolve(string root, string value, string setting)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"{setting} is empty in SprtConfig.json.");
        if (Path.IsPathRooted(value)) throw new InvalidOperationException($"{setting} must be relative, for example \"baseline\". Got \"{value}\".");
        return Path.GetFullPath(Path.Combine(root, value));
    }

    private static string ResolveStatic(string value, string setting)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"{setting} is empty in SprtConfig.json.");
        return Path.GetFullPath(value);
    }
}
