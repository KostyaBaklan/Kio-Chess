namespace Engine.Interfaces.Config;

public class NullConfiguration
{
    /// <summary>Null-search depth indexed by node depth (a target depth, not the reduction R). Must cover every reachable node depth.</summary>
    public sbyte[] NullDepthReduction { get; set; }

    /// <summary>Node depth used after a verified fail-high, indexed by node depth.</summary>
    public sbyte[] NullDepthExtendedReduction { get; set; }

    /// <summary>Minimum number of plies from the root before a null move is allowed.</summary>
    public int NullMinPlyFromRoot { get; set; }

    /// <summary>Null is tried only when static eval + margin >= beta.</summary>
    public int NullEvalMargin { get; set; }
    public int NullEvalMinDepth { get; set; }

    /// <summary>Fail-highs at nodes with depth below this value prune directly; at or above it they are verified by a reduced search.</summary>
    public int NullVerificationDepth { get; set; }
}