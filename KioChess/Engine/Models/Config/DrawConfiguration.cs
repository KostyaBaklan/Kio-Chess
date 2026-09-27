namespace Engine.Models.Config;

/// <summary>
/// Groups all anti-draw / endgame-conversion tuning values used to help the engine
/// convert winning positions instead of drifting into threefold repetition, the
/// 50-move rule, or other draws: contempt bias, mop-up (mating-drive) evaluation,
/// and rule50 decay scaling.
/// </summary>
public class DrawConfiguration
{
    /// <summary>
    /// Small negative-from-side-to-move-perspective bias applied to draw scores
    /// (repetition, 50-move, material draw) in the main search, so the engine
    /// avoids steering into draws when it evaluates itself as ahead. Not applied
    /// during null-move search (see NullConfiguration remarks).
    /// Default: 15 (centipawns)
    /// </summary>
    public short ContemptValue { get; set; }

    /// <summary>
    /// Minimum number of enemy pieces attacking the side-to-move's own king-shield
    /// zone (see Board.IsWhiteKingInDanger/IsBlackKingInDanger) for that side's king
    /// to be considered "in danger". While in danger, contempt is suppressed
    /// (treated as 0) instead of discouraging a draw, since a materially-ahead
    /// side whose king is under a real attack should not be biased away from a
    /// safe repetition/draw escape by static material alone.
    /// Default: 2
    /// </summary>
    public byte ContemptKingDangerAttackers { get; set; }

    public MopUpConfiguration MopUp { get; set; }

    public Rule50DecayConfiguration Rule50Decay { get; set; }
}
