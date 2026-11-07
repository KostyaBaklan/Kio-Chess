namespace Engine.Interfaces.Config;

public interface IBookConfiguration
{
    string DatabasePath { get; }
    short GamesThreshold { get; }
    short SearchDepth { get; }
    short SaveDepth { get; }
    int PopularThreshold { get; }
    int MinimumPopular { get; }
    int MinimumPopularThreshold { get; }
    int MaximumPopularThreshold { get; }
    int PopularDepth { get; }
    int Chunk { get; }
    int EloCount { get;  }
    string PgnPath { get; }
    string FilteredPgnFile { get; }
    PgnFilterRule[] PgnFilterRules { get; }
}

public class PgnFilterRule
{
    public string TimeControl { get; set; }
    public int MinElo { get; set; }
}