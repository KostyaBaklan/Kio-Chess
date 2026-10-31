namespace Engine.Models.Config;

public class NnueLabelConfiguration
{
    public string Strategy { get; set; }
    public short Depth { get; set; }
    public int MaxAbsScore { get; set; }
    public bool SkipCaptureBestMove { get; set; }
    public bool ClearTablePerGame { get; set; }
    public int Workers { get; set; }
    public int RangesPerWorker { get; set; }
    public double MaxHours { get; set; }
    public int FlushPositions { get; set; }
}
