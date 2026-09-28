namespace Engine.Models.Config
{
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

        public MopUpConfiguration MopUp { get; set; }

        public Rule50DecayConfiguration Rule50Decay { get; set; }
    }
    
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
}
