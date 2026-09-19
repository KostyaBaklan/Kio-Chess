namespace Analysis.Core.Models;

/// <summary>
/// Comprehensive analysis report for a chess game.
/// </summary>
public class GameAnalysisReport
{
    public int TotalMoves { get; set; }
    public int AnalysisDepth { get; set; }
    public DateTime AnalysisDate { get; set; }
    
    public List<AnalysedMove> Moves { get; set; } = new();
    
    public PlayerStatistics WhiteStatistics { get; set; } = new();
    public PlayerStatistics BlackStatistics { get; set; } = new();
    
    public PhaseScores PhaseScores { get; set; } = new();
    
    public List<KeyMoment> KeyMoments { get; set; } = new();
    
    /// <summary>Opening name if detected.</summary>
    public string OpeningName { get; set; }
    
    /// <summary>Game result.</summary>
    public string Result { get; set; }
}

/// <summary>
/// Statistics for one player's performance in a game.
/// </summary>
public class PlayerStatistics
{
    public int TotalMoves { get; set; }
    
    public int BestMoves { get; set; }
    public int ExcellentMoves { get; set; }
    public int GoodMoves { get; set; }
    public int BrilliantMoves { get; set; }
    public int BookMoves { get; set; }
    public int Inaccuracies { get; set; }
    public int Mistakes { get; set; }
    public int Blunders { get; set; }
    
    public int TotalCentipawnLoss { get; set; }
    public double AverageCentipawnLoss { get; set; }
    
    /// <summary>Overall accuracy percentage (0-100).</summary>
    public double Accuracy { get; set; }
    
    /// <summary>Display text for accuracy with color.</summary>
    public string AccuracyDisplay => $"{Accuracy:F1}%";
    
    /// <summary>Accuracy color for UI display.</summary>
    public string AccuracyColor => Accuracy switch
    {
        >= 95 => "#7AC142",  // Excellent - Green
        >= 85 => "#96BC4B",  // Good - Light Green
        >= 75 => "#96AF8B",  // Decent - Neutral
        >= 60 => "#F0C15C",  // Below Average - Yellow
        >= 40 => "#E58F2A",  // Poor - Orange
        _ => "#CA3431"       // Very Poor - Red
    };
}

/// <summary>
/// Performance scores for each phase of the game.
/// </summary>
public class PhaseScores
{
    /// <summary>Opening phase score (0-100).</summary>
    public double OpeningScore { get; set; }
    
    /// <summary>Middlegame phase score (0-100).</summary>
    public double MiddlegameScore { get; set; }
    
    /// <summary>Endgame phase score (0-100).</summary>
    public double EndgameScore { get; set; }
    
    /// <summary>White opening phase accuracy (0-100).</summary>
    public double WhiteOpeningAccuracy { get; set; }
    
    /// <summary>Black opening phase accuracy (0-100).</summary>
    public double BlackOpeningAccuracy { get; set; }
    
    /// <summary>White middlegame phase accuracy (0-100).</summary>
    public double WhiteMiddlegameAccuracy { get; set; }
    
    /// <summary>Black middlegame phase accuracy (0-100).</summary>
    public double BlackMiddlegameAccuracy { get; set; }
    
    /// <summary>White endgame phase accuracy (0-100).</summary>
    public double WhiteEndgameAccuracy { get; set; }
    
    /// <summary>Black endgame phase accuracy (0-100).</summary>
    public double BlackEndgameAccuracy { get; set; }
    
    public string OpeningGrade => ScoreToGrade(OpeningScore);
    public string MiddlegameGrade => ScoreToGrade(MiddlegameScore);
    public string EndgameGrade => ScoreToGrade(EndgameScore);
    
    private static string ScoreToGrade(double score) => score switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F"
    };
}

/// <summary>
/// Represents a key moment in the game (brilliant move, blunder, turning point).
/// </summary>
public class KeyMoment
{
    public int MoveNumber { get; set; }
    public bool IsWhite { get; set; }
    public KeyMomentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public AnalysedMove Move { get; set; } = null!;
    
    public string PlayerName => IsWhite ? "White" : "Black";
    
    public string IconGlyph => Type switch
    {
        KeyMomentType.Brilliant => "?",
        KeyMomentType.Blunder => "??",
        KeyMomentType.TurningPoint => "??",
        KeyMomentType.MissedWin => "?",
        _ => "•"
    };
}

public enum KeyMomentType
{
    Brilliant,
    Blunder,
    TurningPoint,
    MissedWin
}

/// <summary>
/// Analysis of a single position (not necessarily from a full game).
/// </summary>
public class PositionAnalysis
{
    public string Fen { get; set; } = string.Empty;
    public int Depth { get; set; }
    public int Evaluation { get; set; }
    public bool IsMate { get; set; }
    public int MateIn { get; set; }
    public string BestMove { get; set; } = string.Empty;
    public string PrincipalVariation { get; set; } = string.Empty;
    public long Nodes { get; set; }
    public int TimeMs { get; set; }
    
    public string EvaluationDisplay => IsMate 
        ? $"Mate in {Math.Abs(MateIn)}"
        : $"{(Evaluation >= 0 ? "+" : "")}{Evaluation / 100.0:F2}";
}
