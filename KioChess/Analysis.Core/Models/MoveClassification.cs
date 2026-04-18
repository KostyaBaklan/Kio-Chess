namespace Analysis.Core.Models;

/// <summary>
/// Classification of a chess move based on its impact on the position evaluation.
/// </summary>
public enum MoveClassification
{
    /// <summary>Not yet analyzed.</summary>
    None,
    
    /// <summary>Best or near-best move (loss ? 10 centipawns).</summary>
    Best,
    
    /// <summary>Excellent move (loss ? 20 centipawns).</summary>
    Excellent,
    
    /// <summary>Good move (loss ? 50 centipawns).</summary>
    Good,
    
    /// <summary>Theoretical opening book move.</summary>
    Book,
    
    /// <summary>Brilliant tactical sacrifice or deep positional idea (evaluation improved significantly).</summary>
    Brilliant,
    
    /// <summary>Acceptable move (loss ? 100 centipawns).</summary>
    Inaccuracy,
    
    /// <summary>Mistake (loss ? 300 centipawns).</summary>
    Mistake,
    
    /// <summary>Blunder (loss > 300 centipawns).</summary>
    Blunder
}

/// <summary>
/// Contains move evaluation details for display and analysis.
/// </summary>
public sealed record MoveEvaluation
{
    /// <summary>Evaluation before the move (from mover's perspective).</summary>
    public int EvalBefore { get; init; }
    
    /// <summary>Evaluation after the move (from mover's perspective).</summary>
    public int EvalAfter { get; init; }
    
    /// <summary>Best move's evaluation in this position.</summary>
    public int BestMoveEval { get; init; }
    
    /// <summary>Best move in UCI notation (e.g., "e2e4").</summary>
    public string BestMove { get; init; } = string.Empty;
    
    /// <summary>The move that was actually played in UCI notation.</summary>
    public string PlayedMove { get; init; } = string.Empty;
    
    /// <summary>Best move sequence (PV line) from this position.</summary>
    public string BestMoveSequence { get; init; } = string.Empty;
    
    /// <summary>Classification of the played move.</summary>
    public MoveClassification Classification { get; init; }
    
    /// <summary>Centipawn loss from best move (0 = best, higher = worse).</summary>
    public int CentipawnLoss => Math.Max(0, BestMoveEval - EvalAfter);
    
    /// <summary>Display text for the classification.</summary>
    public string ClassificationText => Classification switch
    {
        MoveClassification.Brilliant => "!!",
        MoveClassification.Best => "",
        MoveClassification.Excellent => "!",
        MoveClassification.Good => "",
        MoveClassification.Book => "Book",
        MoveClassification.Inaccuracy => "?!",
        MoveClassification.Mistake => "?",
        MoveClassification.Blunder => "??",
        _ => string.Empty
    };
}
