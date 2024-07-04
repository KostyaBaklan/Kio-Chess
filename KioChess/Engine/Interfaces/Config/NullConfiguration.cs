namespace Engine.Interfaces.Config;

public class NullConfiguration
{
    public int NullWindow { get; set; }
    public int NullDepthReduction { get; set; }
    public int NullDepthExtendedReduction { get; set; }
    public int NullDepthThreshold { get; set; }
    public int MaxAdaptiveDepthReduction { get; set; }
    public int MinAdaptiveDepthReduction { get; set; }
    public int AdaptiveDepthThreshold { get; set;}
}