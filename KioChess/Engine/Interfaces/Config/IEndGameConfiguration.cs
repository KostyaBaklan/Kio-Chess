namespace Engine.Interfaces.Config;

public interface IEndGameConfiguration
{
    sbyte[] EndGameDepthOffset { get; }
    sbyte[] EndGameDepth { get; }
    int EndGameSearchExtension { get;  }
    sbyte[] EndGameDepthExtension { get; }
}