namespace Engine.Interfaces.Config;

public class LateMoveConfiguration
{
    public int[] Lmr { get; set; }
    public int[] Lmrd { get; set; }
    public int[] LmrEnd { get; set; }
    public int[] LmrMove { get; set; }
    public int[] LmrLowMove { get; set; }
    public int LmrMoveDepth { get; set; }
    public int[] LmrRatio { get; set; }
    public int[] LmrEndRatio { get; set; }
}