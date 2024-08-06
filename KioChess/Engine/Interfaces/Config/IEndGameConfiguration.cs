namespace Engine.Interfaces.Config;

public interface IEndGameConfiguration
{
    short MaxEndGameDepth { get; }
    sbyte[] EndGameDepthOffset { get; }
}