namespace TexelTool;

public class OptimizerConfiguration
{
    public int WorkerCount { get; set; }
    public double SigmoidScale { get; set; }
    public int KFitIterations { get; set; }
    public double KMin { get; set; }
    public double KMax { get; set; }
    public int MaxEpochsPerStep { get; set; }
    public double MinEpochImprovement { get; set; } = 0.00002;
    public int[] StepSizes { get; set; }
}
