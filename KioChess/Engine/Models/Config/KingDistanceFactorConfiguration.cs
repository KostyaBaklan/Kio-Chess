namespace Engine.Models.Config;

public class KingDistanceFactorConfiguration
{
    /// <summary>
    /// Coefficient for friendly king bonus calculation: (maxDistance - distance) * coefficient
    /// Default: 2
    /// </summary>
    public byte FriendlyKingBonusCoefficient { get; set; }

    /// <summary>
    /// Maximum distance for friendly king bonus (pawns farther than this get 0 bonus)
    /// Default: 4
    /// </summary>
    public byte FriendlyKingMaxDistance { get; set; }

    /// <summary>
    /// Coefficient for enemy king penalty calculation: (maxDistance - distance) * coefficient
    /// Default: 3
    /// </summary>
    public byte EnemyKingPenaltyCoefficient { get; set; }

    /// <summary>
    /// Maximum distance for enemy king penalty (pawns farther than this get 0 penalty)
    /// Default: 3
    /// </summary>
    public byte EnemyKingMaxDistance { get; set; }
}
