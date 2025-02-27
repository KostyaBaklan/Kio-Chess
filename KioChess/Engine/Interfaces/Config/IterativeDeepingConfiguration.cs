namespace Engine.Interfaces.Config;

public class IterativeDeepingConfiguration
{
    public sbyte[] InitialDepth { get; set; }
    public short DepthStep { get; set; }
    public string[] Strategies { get; set; }
}
