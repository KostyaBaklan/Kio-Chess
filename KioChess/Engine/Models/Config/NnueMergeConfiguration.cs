namespace Engine.Models.Config;

public class NnueMergeConfiguration
{
    public double ValidationRate { get; set; }
    public long MaxPoolPositions { get; set; }
    public int Seed { get; set; }
}
