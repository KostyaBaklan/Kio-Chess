namespace Engine.Interfaces.Config;

public interface IEndGameConfiguration
{
    sbyte[] EndGameDepthOffset { get; }
    sbyte[] EndGameDepth { get; }

    /// <summary>
    /// Additional depth extension for pure pawn endgames (K+P vs K+P).
    /// Pawn endgames are highly tactical and often require 20+ ply to solve correctly.
    /// </summary>
    sbyte PawnEndgameDepthExtension { get; }

    /// <summary>
    /// Additional depth extension for late endgames (few pieces remaining).
    /// </summary>
    sbyte LateEndgameDepthExtension { get; }
}