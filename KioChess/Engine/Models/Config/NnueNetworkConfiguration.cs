namespace Engine.Models.Config;

public class NnueNetworkConfiguration
{
    public int Inputs { get; set; }
    public int Hidden { get; set; }
    public int QA { get; set; }
    public int QB { get; set; }
    public int Scale { get; set; }
    public int MaxEvaluation { get; set; }
    public bool UseEvaluationCache { get; set; }
    public bool UseEndGameAdjustments { get; set; }
}
