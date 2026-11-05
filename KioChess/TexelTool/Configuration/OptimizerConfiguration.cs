namespace TexelTool.Configuration
{
    public class OptimizerConfiguration
    {
        public int WorkerCount { get; set; }
        public double SigmoidScale { get; set; }
        public int KFitIterations { get; set; }
        public double KMin { get; set; }
        public double KMax { get; set; }
        public int MaxEpochsPerStep { get; set; }
        public double MinEpochImprovement { get; set; } = 0.00002;
        public double[] MinEpochImprovements { get; set; }
        public int[] StepSizes { get; set; }
    }
}
