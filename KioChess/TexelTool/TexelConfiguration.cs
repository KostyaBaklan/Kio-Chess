namespace TexelTool;

public class TexelConfiguration
{
    public string PgnPath { get; set; }
    public bool DeletePgnAfterExtraction { get; set; }
    public int ProgressIntervalSeconds { get; set; }
    public string OutputDirectory { get; set; }
    public int MinElo { get; set; }
    public int WorkerCount { get; set; }
    public string DatasetFile { get; set; }
    public bool ReuseDataset { get; set; }
    public int MaxGames { get; set; }
    public int MaxPositions { get; set; }
    public int SkipOpeningPlies { get; set; }
    public int MaxPliesPerGame { get; set; }
    public int SampleEveryNthPosition { get; set; }
    public bool ExcludeInCheck { get; set; }
    public bool ExcludeCaptures { get; set; }
    public int MaxAbsStaticScore { get; set; }
    public int OpeningMaxPly { get; set; }
    public int OpeningMinMaterial { get; set; }
    public double SigmoidScale { get; set; }
    public int KFitIterations { get; set; }
    public double KMin { get; set; }
    public double KMax { get; set; }
    public int MaxEpochsPerStep { get; set; }
    public int[] StepSizes { get; set; }
    public bool TuneScalars { get; set; }
    public bool TunePieceValues { get; set; }
    public bool TunePst { get; set; }
    public int ScalarMinValue { get; set; }
    public int ScalarMaxValue { get; set; }
    public int PieceValueMin { get; set; }
    public int PieceValueMax { get; set; }
    public int PstMinValue { get; set; }
    public int PstMaxValue { get; set; }
    public string TunedConfigurationFile { get; set; }
    public string TunedStaticTablesFile { get; set; }
}
