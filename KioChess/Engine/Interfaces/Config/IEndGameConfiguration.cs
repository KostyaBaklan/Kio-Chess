namespace Engine.Interfaces.Config;

public interface IEndGameConfiguration
{
    sbyte[] EndGameDepthOffset { get; }
    sbyte[] EndGameDepth { get; }
}