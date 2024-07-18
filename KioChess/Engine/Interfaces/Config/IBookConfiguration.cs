namespace Engine.Interfaces.Config;

public interface IBookConfiguration
{
    short GamesThreshold { get; }
    short SearchDepth { get; }
    short SaveDepth { get; }
    short Elo { get;  }
    int PopularThreshold { get; }
    int MinimumPopular { get; }
    int MinimumPopularThreshold { get; }
    int MaximumPopularThreshold { get; }
    int PopularDepth { get; }
    int Chunk { get;  }
}