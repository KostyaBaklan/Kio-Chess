namespace Engine.Models.Config;

/// <summary>
/// Configuration for "50-move rule decay": progressively scales down the End-phase
/// static evaluation as the reversible-move (halfmove clock) counter grows, so a
/// large material/positional advantage loses weight the closer the position gets
/// to an automatic 50-move draw. This creates search pressure to prefer moves that
/// reset the counter (captures, pawn pushes) or make real progress (see MopUp)
/// well before the hard cutoff, instead of only reacting once the draw is imminent.
/// </summary>
public class Rule50DecayConfiguration
{
    /// <summary>
    /// Reversible-move ply count (0-99) at which decay starts. Below this count the
    /// evaluation is unscaled (numerator = 1000/1.0).
    /// Default: 40
    /// </summary>
    public byte StartPly { get; set; }

    /// <summary>
    /// Reversible-move ply count (0-99) at which decay reaches its minimum scale
    /// (MinScaleNumerator). Between StartPly and FullDecayPly the scale decreases
    /// linearly.
    /// Default: 90
    /// </summary>
    public byte FullDecayPly { get; set; }

    /// <summary>
    /// Minimum fixed-point scale numerator (denominator is 1000), reached at/after
    /// FullDecayPly. Must be in [0, 1000]. Default: 250 (i.e. 25% of original eval)
    /// </summary>
    public short MinScaleNumerator { get; set; }
}
