namespace Engine.Interfaces.Config;

public interface IBookConfiguration
{
    Dictionary<string, string> Connection { get; }
    Dictionary<string, string> Connection1 { get; }
    Dictionary<string, string> Connection2 { get; }
    Dictionary<string, string> ConnectionT { get; }

    short SuggestedThreshold { get; }

    short NonSuggestedThreshold { get; }
    short GamesThreshold { get; }
    short SearchDepth { get; }
    short SaveDepth { get; }
    short Elo { get;  }
    bool UseBooking { get; }
    int PopularThreshold { get; }
    int MinimumPopular { get; }
    int MinimumPopularThreshold { get; }
    int PopularDepth { get; }
    int Chunk { get;  }
}