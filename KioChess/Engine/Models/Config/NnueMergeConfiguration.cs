namespace Engine.Models.Config;

public class NnueMergeConfiguration
{
    public double ValidationRate { get; set; }
    public int ShuffleChunkMB { get; set; }
    public long MaxPoolPositions { get; set; }
}
