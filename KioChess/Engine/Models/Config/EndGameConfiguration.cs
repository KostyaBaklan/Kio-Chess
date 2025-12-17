using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class EndGameConfiguration : IEndGameConfiguration
{
    public sbyte[] EndGameDepthOffset { get; set; }

    public sbyte[] EndGameDepth { get; set; }

    /// <summary>
    /// Additional depth extension for pure pawn endgames (K+P vs K+P).
    /// Pawn endgames are highly tactical and often require 20+ ply to solve correctly.
    /// </summary>
    public sbyte PawnEndgameDepthExtension { get; set; }

    /// <summary>
    /// Additional depth extension for late endgames (few pieces remaining).
    /// </summary>
    public sbyte LateEndgameDepthExtension { get; set; }
}