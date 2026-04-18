using Analysis.KioUI.Models;

namespace Analysis.KioUI.ViewModels;

/// <summary>
/// Represents one square on the chess board.
/// Cell indices follow the engine convention: 0 = A1, 1 = B1 … 63 = H8.
/// </summary>
public class CellViewModel : BindableBase
{
    public CellViewModel(byte cell, CellType cellType)
    {
        Cell = cell;
        CellType = cellType;
    }

    // ?? Identity (immutable) ?????????????????????????????????????
    public byte Cell { get; }
    public CellType CellType { get; }

    // ?? Piece ????????????????????????????????????????????????????
    private byte? _figure;
    public byte? Figure
    {
        get => _figure;
        set => SetProperty(ref _figure, value);
    }

    // ?? Selection / highlight states ?????????????????????????????
    private bool _isSelected;
    /// <summary>The square from which the user is moving.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private bool _isValidTarget;
    /// <summary>Legal destination for the currently selected piece.</summary>
    public bool IsValidTarget
    {
        get => _isValidTarget;
        set => SetProperty(ref _isValidTarget, value);
    }

    private bool _isLastMoveFrom;
    /// <summary>The origin square of the last move played.</summary>
    public bool IsLastMoveFrom
    {
        get => _isLastMoveFrom;
        set => SetProperty(ref _isLastMoveFrom, value);
    }

    private bool _isLastMoveTo;
    /// <summary>The destination square of the last move played.</summary>
    public bool IsLastMoveTo
    {
        get => _isLastMoveTo;
        set => SetProperty(ref _isLastMoveTo, value);
    }

    private bool _isInCheck;
    /// <summary>The king on this square is in check.</summary>
    public bool IsInCheck
    {
        get => _isInCheck;
        set => SetProperty(ref _isInCheck, value);
    }

    // ?? Helpers ??????????????????????????????????????????????????
    public void ClearHighlights()
    {
        IsSelected = false;
        IsValidTarget = false;
        IsLastMoveFrom = false;
        IsLastMoveTo = false;
        IsInCheck = false;
    }
}
