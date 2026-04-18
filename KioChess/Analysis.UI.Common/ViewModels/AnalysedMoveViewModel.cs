using Analysis.Core.Models;

namespace Analysis.UI.Common.ViewModels;

/// <summary>
/// ViewModel for displaying an individual analyzed move in the analysis report.
/// </summary>
public class AnalysedMoveViewModel : BindableBase
{
    private readonly AnalysedMove _analysedMove;

    public AnalysedMoveViewModel(AnalysedMove analysedMove, Action<int> jumpToMoveAction)
    {
        _analysedMove = analysedMove;
        
        // Calculate the actual move index (0-based) from move number and color
        int moveIndex = (MoveNumber - 1) * 2 + (IsWhite ? 0 : 1);
        JumpToMoveCommand = new DelegateCommand(() => jumpToMoveAction(moveIndex));
    }

    public DelegateCommand JumpToMoveCommand { get; }

    public int MoveNumber => _analysedMove.MoveNumber;
    public bool IsWhite => _analysedMove.IsWhite;
    public string Notation => _analysedMove.Notation;
    public string PlayerName => _analysedMove.IsWhite ? "White" : "Black";
    
    public int EvaluationBefore => _analysedMove.EvaluationBefore;
    public int EvaluationAfter => _analysedMove.EvaluationAfter;
    public int CentipawnLoss => _analysedMove.CentipawnLoss;
    
    public string EvaluationBeforeDisplay => FormatEvaluation(_analysedMove.EvaluationBefore);
    public string EvaluationAfterDisplay => FormatEvaluation(_analysedMove.EvaluationAfter);
    
    public MoveClassification Classification => _analysedMove.Classification;
    public string ClassificationText => Classification switch
    {
        MoveClassification.Brilliant => "Brilliant!!",
        MoveClassification.Best => "Best",
        MoveClassification.Excellent => "Excellent!",
        MoveClassification.Good => "Good",
        MoveClassification.Book => "Book",
        MoveClassification.Inaccuracy => "Inaccuracy",
        MoveClassification.Mistake => "Mistake",
        MoveClassification.Blunder => "Blunder!!",
        _ => string.Empty
    };
    
    public string ClassificationColor => _analysedMove.ClassificationColor;
    
    public string ClassificationSymbol => Classification switch
    {
        MoveClassification.Brilliant => "!!",
        MoveClassification.Excellent => "!",
        MoveClassification.Inaccuracy => "?!",
        MoveClassification.Mistake => "?",
        MoveClassification.Blunder => "??",
        _ => string.Empty
    };
    
    public string BestMove => _analysedMove.BestMove;
    public string BestMoveSequence => _analysedMove.BestMoveSequence;
    
    public bool IsBestMoveDifferent => !string.IsNullOrEmpty(BestMove) && 
                                       Classification != MoveClassification.Best &&
                                       Classification != MoveClassification.Brilliant;
    
    public string CentipawnLossDisplay => CentipawnLoss > 0 ? $"-{CentipawnLoss}" : string.Empty;
    
    public string BestMoveSequenceDisplay
    {
        get
        {
            if (!IsBestMoveDifferent || string.IsNullOrEmpty(BestMoveSequence))
                return string.Empty;
            
            var moves = BestMoveSequence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (moves.Length == 0)
                return string.Empty;
            
            var formatted = string.Join(" ", moves.Take(Math.Min(4, moves.Length)));
            return formatted;
        }
    }
    
    public bool HasBestMoveSequence => !string.IsNullOrEmpty(BestMoveSequenceDisplay);
    
    public string MoveDisplay => IsWhite 
        ? $"{MoveNumber}. {Notation}" 
        : $"{MoveNumber}... {Notation}";
    
    private bool _isFiltered;
    public bool IsFiltered
    {
        get => _isFiltered;
        set => SetProperty(ref _isFiltered, value);
    }
    
    private bool _isCurrentMove;
    public bool IsCurrentMove
    {
        get => _isCurrentMove;
        set => SetProperty(ref _isCurrentMove, value);
    }
    
    private static string FormatEvaluation(int centipawns)
    {
        double pawns = centipawns / 100.0;
        return pawns >= 0 ? $"+{pawns:F2}" : $"{pawns:F2}";
    }
}
