namespace Engine.Interfaces.Config;

public class ExtensionConfiguration
{
    public bool IsPvEnabled { get; set; }
    public int[] DepthDifference { get; set; }
    public int[] EndDepthDifference { get; set; }
    public int[] EvaluationOffest { get; set; }
    public int NullEvaluationOffest { get; set; }
}