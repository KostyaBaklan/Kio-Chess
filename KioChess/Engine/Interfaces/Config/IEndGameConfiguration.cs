namespace Engine.Interfaces.Config
{
    public interface IEndGameConfiguration
    {
        int MaxEndGameDepth { get; }
        int EndGameDepthOffset { get; }
    }
}