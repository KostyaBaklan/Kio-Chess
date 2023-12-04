namespace Engine.Interfaces.Config;

public interface IEndGameConfiguration
{
    short MaxEndGameDepth { get; }
    short EndGameDepthOffset { get; }
}