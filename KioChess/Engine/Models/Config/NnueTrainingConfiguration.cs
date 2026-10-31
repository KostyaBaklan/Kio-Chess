namespace Engine.Models.Config;

public class NnueTrainingConfiguration
{
    public string TrainPath { get; set; }
    public string ValidationPath { get; set; }
    public string OutputDirectory { get; set; }
    public int Threads { get; set; }
    public int BatchSize { get; set; }
    public int Epochs { get; set; }
    public double LearningRate { get; set; }
    public double FinalLearningRate { get; set; }
    public double WeightDecay { get; set; }
    public double Lambda { get; set; }
    public double WeightClip { get; set; }
    public int Seed { get; set; }
    public double MaxHours { get; set; }
}
