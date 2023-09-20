namespace Engine.Interfaces.Config
{
    public interface IBookConfiguration
    {
        Dictionary<string, string> Connection { get; }

        short SuggestedThreshold { get; }

        short NonSuggestedThreshold { get; }
        short GamesThreshold { get; }
        short SearchDepth { get; }
        short SaveDepth { get; }
        short Elo { get;  }
        bool UseBooking { get; }
    }
}