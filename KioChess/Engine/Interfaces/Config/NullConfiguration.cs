namespace Engine.Interfaces.Config;

public class NullConfiguration
{
    public int NullWindow { get; set; }
    public sbyte[] NullDepthReduction { get; set; }
    public sbyte[] NullDepthExtendedReduction { get; set; }
    public int NullDepthThreshold { get; set; }

    /// <summary>
    /// Reversible-move (halfmove clock) count at/above which null-move pruning is
    /// disabled. Near the 50-move mark, a null-move ("pass") search doesn't reflect
    /// the real risk of running out the clock before making progress, so its
    /// result can look artificially safe and cause over-pruning in technical
    /// endgames where the side to move must find real progress in time.
    /// Default: 85 (out of 99).
    /// </summary>
    public int NullMoveRule50Threshold { get; set; }
}
