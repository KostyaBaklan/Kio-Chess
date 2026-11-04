using Newtonsoft.Json;

namespace TexelTool;

public class TexelConfiguration
{
    public int ProgressIntervalSeconds { get; set; }
    public PathsConfiguration Paths { get; set; } = new();
    public DatasetConfiguration Dataset { get; set; } = new();
    public OptimizerConfiguration Optimizer { get; set; } = new();
    public TuningConfiguration Tuning { get; set; } = new();
    public BoundsConfiguration Bounds { get; set; } = new();

    [JsonIgnore] public string PgnPath => Paths.PgnPath;
    [JsonIgnore] public string OutputDirectory => Paths.OutputDirectory;
    [JsonIgnore] public string DatasetFile => Paths.DatasetFile;
    [JsonIgnore] public string TunedConfigurationFile => Paths.TunedConfigurationFile;
    [JsonIgnore] public string TunedStaticTablesFile => Paths.TunedStaticTablesFile;

    [JsonIgnore] public bool DeletePgnAfterExtraction => Dataset.DeletePgnAfterExtraction;
    [JsonIgnore] public bool ReuseDataset => Dataset.ReuseDataset;
    [JsonIgnore] public int MinElo => Dataset.MinElo;
    [JsonIgnore] public int MaxGames => Dataset.MaxGames;
    [JsonIgnore] public int MaxPositions => Dataset.MaxPositions;
    [JsonIgnore] public int SkipOpeningPlies => Dataset.SkipOpeningPlies;
    [JsonIgnore] public int MaxPliesPerGame => Dataset.MaxPliesPerGame;
    [JsonIgnore] public int SampleEveryNthPosition => Dataset.SampleEveryNthPosition;
    [JsonIgnore] public bool ExcludeInCheck => Dataset.ExcludeInCheck;
    [JsonIgnore] public bool ExcludeCaptures => Dataset.ExcludeCaptures;
    [JsonIgnore] public int MaxAbsStaticScore => Dataset.MaxAbsStaticScore;
    [JsonIgnore] public int ValidationEvery => Dataset.ValidationEvery;

    [JsonIgnore] public int WorkerCount => Optimizer.WorkerCount;
    [JsonIgnore] public double SigmoidScale => Optimizer.SigmoidScale;
    [JsonIgnore] public int KFitIterations => Optimizer.KFitIterations;
    [JsonIgnore] public double KMin => Optimizer.KMin;
    [JsonIgnore] public double KMax => Optimizer.KMax;
    [JsonIgnore] public int MaxEpochsPerStep => Optimizer.MaxEpochsPerStep;
    [JsonIgnore] public double MinEpochImprovement => Optimizer.MinEpochImprovement;
    [JsonIgnore] public int[] StepSizes => Optimizer.StepSizes;

    [JsonIgnore]
    public bool UseIncrementalLinear
    {
        get => Tuning.UseIncrementalLinear;
        set => Tuning.UseIncrementalLinear = value;
    }
    [JsonIgnore] public bool DeleteDatasetOnSuccess => Tuning.DeleteDatasetOnSuccess;
    [JsonIgnore] public bool TuneScalars => Tuning.TuneScalars;
    [JsonIgnore] public bool TunePieceValues => Tuning.TunePieceValues;
    [JsonIgnore] public bool TunePst => Tuning.TunePst;
    [JsonIgnore] public bool TuneKingSafety => Tuning.TuneKingSafety;
    [JsonIgnore] public bool RecentreTables => Tuning.RecentreTables;
    [JsonIgnore] public bool TunePassedPawns => Tuning.TunePassedPawns;

    [JsonIgnore] public int KingSafetyMaxValue => Bounds.KingSafetyMaxValue;
    [JsonIgnore] public int PassedPawnMaxValue => Bounds.PassedPawnMaxValue;
    [JsonIgnore] public int ScalarMinValue => Bounds.ScalarMinValue;
    [JsonIgnore] public int ScalarMaxValue => Bounds.ScalarMaxValue;
    [JsonIgnore] public int PieceValueMin => Bounds.PieceValueMin;
    [JsonIgnore] public int PieceValueMax => Bounds.PieceValueMax;
    [JsonIgnore] public int PstMinValue => Bounds.PstMinValue;
    [JsonIgnore] public int PstMaxValue => Bounds.PstMaxValue;
}
