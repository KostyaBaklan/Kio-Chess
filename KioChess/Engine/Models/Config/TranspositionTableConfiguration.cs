namespace Engine.Models.Config;

public class TranspositionTableConfiguration
{
    public int DepthFactor { get; set; }

    public int TypeFactor { get; set; }

    public sbyte[] CutoffDepth { get; set; }

    public int[] MainTableSize { get; set; }

    public int[] NullTableSize { get; set; }
}
