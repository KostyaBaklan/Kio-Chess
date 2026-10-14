namespace Engine.Interfaces.Config;

public class NullConfiguration
{
    public int NullWindow { get; set; }
    public sbyte[] NullDepthReduction { get; set; }
    public sbyte[] NullDepthExtendedReduction { get; set; }
    public int NullDepthThreshold { get; set; }
    public int NullEvalMargin { get; set; }
    public int NullEvalMinDepth { get; set; }
}