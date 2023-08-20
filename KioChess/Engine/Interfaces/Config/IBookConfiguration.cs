namespace Engine.Interfaces.Config
{
    public interface IBookConfiguration
    {
        Dictionary<string, string> Connection { get; }

        short SuggestedThreshold { get; }

        short NonSuggestedThreshold { get; }
        short Depth { get; }
    }
}