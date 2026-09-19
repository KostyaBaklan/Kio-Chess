namespace Analysis.Core.Models;

/// <summary>
/// Configuration options for the Stockfish engine.
/// </summary>
public sealed class StockfishOptions
{
    /// <summary>Hash table size in megabytes (default: 64).</summary>
    public int HashSizeMB { get; init; } = 64;
    
    /// <summary>Number of CPU threads to use (default: 1).</summary>
    public int Threads { get; init; } = 1;
    
    /// <summary>Number of principal variations to analyze (default: 1, max typically 5).</summary>
    public int MultiPV { get; init; } = 1;
    
    /// <summary>When true, maintains position history for better evaluation.</summary>
    public bool UsePositionHistory { get; init; } = true;
    
    /// <summary>Analysis depth for move evaluation (default: 18).</summary>
    public int EvaluationDepth { get; init; } = 18;
}
