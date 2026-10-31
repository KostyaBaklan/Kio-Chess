namespace Engine.Models.Config;

public class NnueExtractConfiguration
{
    public int MinPly { get; set; }
    public int MaxPly { get; set; }
    public double SampleRate { get; set; }
    public bool SkipInCheck { get; set; }
    public bool SkipNonQuiet { get; set; }
    public int BloomFilterBits { get; set; }
    public int Seed { get; set; }
}
