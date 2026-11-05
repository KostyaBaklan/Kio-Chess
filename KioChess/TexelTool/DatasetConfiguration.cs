namespace TexelTool;

public class DatasetConfiguration
{
    public bool DeletePgnAfterExtraction { get; set; }
    public bool ReuseDataset { get; set; }
    public int MinElo { get; set; }
    public int MaxGames { get; set; }
    public int MaxPositions { get; set; }
    public int SkipOpeningPlies { get; set; }
    public int SkipOpeningPliesMax { get; set; }
    public int MaxPliesPerGame { get; set; }
    public int SampleEveryNthPosition { get; set; }
    public bool ExcludeInCheck { get; set; }
    public bool ExcludeCaptures { get; set; }
    public int MaxAbsStaticScore { get; set; }
    public int ValidationEvery { get; set; } = 10;
}
