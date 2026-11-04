namespace TexelTool;

public class TuningConfiguration
{
    public bool UseIncrementalLinear { get; set; } = true;
    public bool DeleteDatasetOnSuccess { get; set; } = true;
    public bool TuneScalars { get; set; }
    public bool TunePieceValues { get; set; }
    public bool TunePst { get; set; }
    public bool TuneKingSafety { get; set; }
    public bool RecentreTables { get; set; }
    public bool TunePassedPawns { get; set; }
}
