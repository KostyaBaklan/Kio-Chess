namespace Engine.Models.Config;

/// <summary>
/// Configuration for "mop-up" mating-drive evaluation, active in decisive pawnless
/// endgames (e.g. K+Q vs K, K+R vs K, K+BB vs K). Pushes the losing king toward the
/// edge/corner and rewards the winning king approaching it, to help convert material
/// advantage into checkmate instead of drifting into 50-move/repetition draws.
/// </summary>
public class MopUpConfiguration
{
    /// <summary>
    /// Coefficient applied to (7 - centerManhattanDistance) of the losing king,
    /// i.e. reward for the losing king being close to the corner/edge.
    /// Default: 10
    /// </summary>
    public byte CornerDistanceCoefficient { get; set; }

    /// <summary>
    /// Coefficient applied to (14 - kingDistance) between winning and losing kings,
    /// i.e. reward for the winning king approaching the losing king.
    /// Default: 4
    /// </summary>
    public byte KingDistanceCoefficient { get; set; }

    /// <summary>
    /// Minimum material advantage (centipawns, non-king pieces) required for the
    /// mop-up term to activate. Below this threshold the term contributes 0.
    /// Default: 400 (roughly a minor piece + pawn margin above "clearly winning")
    /// </summary>
    public short MinAdvantageThreshold { get; set; }

    /// <summary>
    /// Hard cap on the total mop-up bonus/penalty to avoid overwhelming material/
    /// positional evaluation terms. Default: 100
    /// </summary>
    public short MaxBonus { get; set; }
}
