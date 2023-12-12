namespace Engine.Interfaces.Config;

public class IterativeDeepingConfiguration
{
    public short InitialDepth { get; set; }
    public short DepthStep { get; set; }
    public string[] Strategies { get; set; }
}
