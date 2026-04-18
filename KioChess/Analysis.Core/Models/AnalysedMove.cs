using Engine.Models.Moves;

namespace Analysis.Core.Models;

/// <summary>
/// Detailed analysis of a single move within a game.
/// </summary>
public class AnalysedMove
{
    public int MoveNumber { get; set; }
    public bool IsWhite { get; set; }
    public MoveBase Move { get; set; } = null!;
    public string Notation { get; set; } = string.Empty;
    
    /// <summary>Evaluation before the move (from mover's perspective).</summary>
    public int EvaluationBefore { get; set; }
    
    /// <summary>Evaluation after the move (from mover's perspective).</summary>
    public int EvaluationAfter { get; set; }
    
    /// <summary>Best move in this position.</summary>
    public string BestMove { get; set; } = string.Empty;
    
    /// <summary>Best move sequence (PV line) from this position.</summary>
    public string BestMoveSequence { get; set; } = string.Empty;
    
    /// <summary>Evaluation if best move was played.</summary>
    public int BestMoveEvaluation { get; set; }
    
    /// <summary>Move classification (Best, Good, Brilliant, Blunder, etc.).</summary>
    public MoveClassification Classification { get; set; }
    
    /// <summary>Centipawns lost by not playing the best move.</summary>
    public int CentipawnLoss { get; set; }
    
    /// <summary>Game phase when this move was played.</summary>
    public GamePhase Phase { get; set; }
    
    /// <summary>Display color for the move (based on classification).</summary>
    public string ClassificationColor => Classification switch
    {
        MoveClassification.Brilliant => "#1BACA6",      // Teal/Cyan (chess.com style)
        MoveClassification.Best => "#9BC53D",           // Lime green
        MoveClassification.Excellent => "#5AC18E",      // Green-cyan
        MoveClassification.Good => "#96AF8B",           // Neutral green
        MoveClassification.Book => "#A88865",           // Brown/tan for book moves
        MoveClassification.Inaccuracy => "#F0C15C",     // Yellow
        MoveClassification.Mistake => "#E58F2A",        // Orange
        MoveClassification.Blunder => "#CA3431",        // Red
        _ => "#CCCCCC"
    };
}

/// <summary>
/// Game phases for chess analysis.
/// </summary>
public enum GamePhase
{
    Opening,
    Middlegame,
    Endgame
}
