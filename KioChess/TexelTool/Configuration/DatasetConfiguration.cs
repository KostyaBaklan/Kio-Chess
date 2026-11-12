namespace TexelTool.Configuration
{
    public class DatasetConfiguration
    {
        public bool DeletePgnAfterExtraction { get; set; }
        public bool ReuseDataset { get; set; }
        public int MinElo { get; set; }
        public int MinEloBlitz { get; set; }
        public int MinEloRapid { get; set; }
        public int MinEloClassical { get; set; }
        public int MinEstimatedGameSeconds { get; set; }
        public int MaxGames { get; set; }
        public int MaxPositions { get; set; }
        public int SkipOpeningPlies { get; set; }
        public int SkipOpeningPliesMax { get; set; }
        public int MaxPliesPerGame { get; set; }
        public int SampleEveryNthPosition { get; set; }
        public bool ExcludeInCheck { get; set; }
        public bool ExcludeCaptures { get; set; }
        public int MaxAbsStaticScore { get; set; }
        public int ValidationEvery { get; set; } = 10;
        public bool RequireNormalTermination { get; set; } = true;
        public bool ExcludeNoisyNextMove { get; set; } = true; 
        public int SampleJitter { get; set; } = 5;
        public bool ExcludeNextMoveCheck { get; set; } = true; 
        public int QuietMargin { get; set; } = 0;       // -1 disables the qsearch filter
        public int QuietProbeDepth { get; set; } = 6;
    }
}
