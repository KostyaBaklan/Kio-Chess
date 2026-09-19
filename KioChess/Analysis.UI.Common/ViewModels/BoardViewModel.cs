using Analysis.UI.Common.Models;
using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using System.Collections.ObjectModel;

namespace Analysis.UI.Common.ViewModels;

/// <summary>
/// Owns all 64 <see cref="CellViewModel"/>s and the board interaction state.
/// Shared between all Analysis.Kio.* applications.
/// </summary>
public class BoardViewModel : BindableBase
{
    private readonly CellViewModel[] _cells = new CellViewModel[64];
    private Position _position;
    private CellViewModel _selectedCell;
    private List<MoveBase> _legalMovesFromSelected = [];

    public BoardViewModel()
    {
        Cells = new ObservableCollection<CellViewModel>(BuildCells());
    }

    /// <summary>64 cells in display order (rank 8 down to rank 1, file A to H).</summary>
    public ObservableCollection<CellViewModel> Cells { get; }

    private BoardState _boardState = BoardState.ReadOnly;
    public BoardState BoardState
    {
        get => _boardState;
        set => SetProperty(ref _boardState, value);
    }

    private bool _isFlipped;
    /// <summary>When true the board is shown from Black's perspective.</summary>
    public bool IsFlipped
    {
        get => _isFlipped;
        set
        {
            if (SetProperty(ref _isFlipped, value))
                RefreshCellOrder();
        }
    }

    private GameResult _gameResult = GameResult.Continue;
    public GameResult GameResult
    {
        get => _gameResult;
        set => SetProperty(ref _gameResult, value);
    }

    /// <summary>Raised after the user selects a legal destination square.</summary>
    public event Action<MoveBase> MoveMade;

    /// <summary>
    /// Loads an existing <see cref="Position"/> into the board display.
    /// </summary>
    public void LoadPosition(Position position)
    {
        _position = position;
        SyncFromPosition();
        ClearAllHighlights();
        GameResult = GameResult.Continue;
    }

    /// <summary>
    /// Re-reads all piece positions from the engine <see cref="Position"/>
    /// and updates the cell ViewModels.
    /// </summary>
    public void SyncFromPosition()
    {
        if (_position is null) return;
        for (byte i = 0; i < 64; i++)
        {
            _position.GetPiece(i, out byte? piece);
            _cells[i].Figure = piece;
        }
    }

    /// <summary>
    /// Handles a cell click. Returns a <see cref="MoveBase"/> if a legal move was
    /// completed, otherwise null.
    /// </summary>
    public MoveBase HandleCellClick(CellViewModel clicked)
    {
        if (BoardState != BoardState.Interactive || _position == null)
            return null;

        // First click: select a piece
        if (_selectedCell == null)
        {
            if (clicked.Figure == null) return null;

            var turn = _position.GetTurn();
            bool isWhitePiece = clicked.Figure.Value < Pieces.BlackPawn;
            if (turn == Turn.White && !isWhitePiece) return null;
            if (turn == Turn.Black && isWhitePiece) return null;

            SelectCell(clicked);
            return null;
        }

        // Second click: attempt move
        if (clicked == _selectedCell)
        {
            ClearAllHighlights();
            _selectedCell = null;
            _legalMovesFromSelected.Clear();
            return null;
        }

        var legalMove = _legalMovesFromSelected.FirstOrDefault(m => m.To == clicked.Cell);
        if (legalMove is not null)
        {
            ClearAllHighlights();
            _selectedCell = null;
            _legalMovesFromSelected.Clear();
            MoveMade?.Invoke(legalMove);
            return legalMove;
        }

        // Clicked a different friendly piece - re-select
        if (clicked.Figure != null)
        {
            var turn = _position.GetTurn();
            bool isWhitePiece = clicked.Figure.Value < Pieces.BlackPawn;
            if ((turn == Turn.White && isWhitePiece) || (turn == Turn.Black && !isWhitePiece))
            {
                ClearAllHighlights();
                SelectCell(clicked);
                return null;
            }
        }

        ClearAllHighlights();
        _selectedCell = null;
        _legalMovesFromSelected.Clear();
        return null;
    }

    /// <summary>
    /// Applies a move directly to the board (used by the engine response path).
    /// </summary>
    public void ApplyEngineMove(MoveBase move)
    {
        var from = move.From;
        var to = move.To;

        ClearAllHighlights();
        move.Make();
        SyncFromPosition();
        SetLastMoveHighlight(from, to);
        UpdateCheckHighlight();
    }

    /// <summary>Undoes the last move and resyncs the display.</summary>
    public void UndoLastMove(MoveBase move, byte previousFrom, byte previousTo)
    {
        move.UnMake();
        SyncFromPosition();
        ClearAllHighlights();
        if (previousFrom < 64 && previousTo < 64)
            SetLastMoveHighlight(previousFrom, previousTo);
    }

    private void SelectCell(CellViewModel cell)
    {
        _selectedCell = cell;
        cell.IsSelected = true;

        if (_position != null)
        {
            _legalMovesFromSelected = _position
                .GetAllMoves(cell.Cell, cell.Figure.Value)
                .ToList();

            foreach (var move in _legalMovesFromSelected)
                _cells[move.To].IsValidTarget = true;
        }
    }

    private void ClearAllHighlights()
    {
        foreach (var c in _cells)
        {
            c.IsSelected = false;
            c.IsValidTarget = false;
            c.IsInCheck = false;
            c.IsLastMoveFrom = false;
            c.IsLastMoveTo = false;
        }
    }

    private void SetLastMoveHighlight(byte from, byte to)
    {
        _cells[from].IsLastMoveFrom = true;
        _cells[to].IsLastMoveTo = true;
    }

    public void SetLastMoveHighlightPublic(byte from, byte to) => SetLastMoveHighlight(from, to);

    private void UpdateCheckHighlight()
    {
        if (_position == null) return;

        var turn = _position.GetTurn();
        var kingPiece = turn == Turn.White ? Pieces.WhiteKing : Pieces.BlackKing;

        for (byte i = 0; i < 64; i++)
        {
            if (_cells[i].Figure == kingPiece)
            {
                _cells[i].IsInCheck = false;
            }
        }
    }

    private CellViewModel[] BuildCells()
    {
        for (byte i = 0; i < 64; i++)
        {
            byte file = (byte)(i % 8);
            byte rank = (byte)(i / 8);
            var cellType = (file + rank) % 2 == 1 ? CellType.Light : CellType.Dark;
            _cells[i] = new CellViewModel(i, cellType);
        }
        return BuildDisplayOrder();
    }

    private CellViewModel[] BuildDisplayOrder()
    {
        var display = new CellViewModel[64];
        int idx = 0;
        if (!_isFlipped)
        {
            for (int rank = 7; rank >= 0; rank--)
                for (int file = 0; file < 8; file++)
                    display[idx++] = _cells[rank * 8 + file];
        }
        else
        {
            for (int rank = 0; rank <= 7; rank++)
                for (int file = 7; file >= 0; file--)
                    display[idx++] = _cells[rank * 8 + file];
        }
        return display;
    }

    private void RefreshCellOrder()
    {
        var ordered = BuildDisplayOrder();
        Cells.Clear();
        foreach (var c in ordered)
            Cells.Add(c);

        RaisePropertyChanged(nameof(RankLabels));
        RaisePropertyChanged(nameof(FileLabels));
    }

    /// <summary>Eight rank labels in display order (top to bottom).</summary>
    public IReadOnlyList<string> RankLabels =>
        _isFlipped
            ? ["1", "2", "3", "4", "5", "6", "7", "8"]
            : ["8", "7", "6", "5", "4", "3", "2", "1"];

    /// <summary>Eight file labels in display order (left to right).</summary>
    public IReadOnlyList<string> FileLabels =>
        _isFlipped
            ? ["h", "g", "f", "e", "d", "c", "b", "a"]
            : ["a", "b", "c", "d", "e", "f", "g", "h"];
}
