using Analysis.DataAccess.Interfaces;
using Analysis.KioUI.Models;
using Engine.Dal.Interfaces;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Analysis.KioUI.ViewModels;

/// <summary>
/// ViewModel for the Opening Library view.
/// Provides opening exploration with move statistics using the IGameDbService.
/// Uses the shared Position singleton but clears it on initialization/reset.
/// </summary>
public class LibraryViewModel : BindableBase
{
    private readonly IGameDbService _gameDbService;
    private readonly ILocalDbService _localDbService;
    private readonly IOpeningExplorerService _openingExplorer;
    private readonly IMoveFormatter _moveFormatter;
    private readonly MoveHistoryService _moveHistoryService;
    private readonly short _searchDepth;
    private readonly Position _position;

    public LibraryViewModel(
        IGameDbService gameDbService,
        ILocalDbService localDbService,
        IOpeningExplorerService openingExplorer,
        IMoveFormatter moveFormatter,
        MoveHistoryService moveHistoryService,
        IConfigurationProvider configurationProvider,
        Position position)
    {
        _gameDbService = gameDbService;
        _localDbService = localDbService;
        _openingExplorer = openingExplorer;
        _moveFormatter = moveFormatter;
        _moveHistoryService = moveHistoryService;
        _searchDepth = configurationProvider.BookConfiguration.SaveDepth;
        _position = position;

        MoveItems = new ObservableCollection<LibraryMoveModel>();
        OpeningMoves = new ObservableCollection<LibraryMoveStatModel>();

        Board = new BoardViewModel { BoardState = BoardState.ReadOnly };
        
        // Clear position to starting state and load into board
        _position.Clear();
        Board.LoadPosition(_position);
        Board.SyncFromPosition();

        InitializeCommands();
        _ = InitializeAsync();
    }

    #region Properties

    public BoardViewModel Board { get; }
    public ObservableCollection<LibraryMoveModel> MoveItems { get; }
    public ObservableCollection<LibraryMoveStatModel> OpeningMoves { get; }

    private string _currentOpeningName = "Starting Position";
    public string CurrentOpeningName
    {
        get => _currentOpeningName;
        set => SetProperty(ref _currentOpeningName, value);
    }

    private string _moveHistoryText = string.Empty;
    public string MoveHistoryText
    {
        get => _moveHistoryText;
        set => SetProperty(ref _moveHistoryText, value);
    }

    private int _depth;
    public int Depth
    {
        get => _depth;
        set => SetProperty(ref _depth, value);
    }

    private bool _isInBook = true;
    public bool IsInBook
    {
        get => _isInBook;
        set => SetProperty(ref _isInBook, value);
    }

    private bool _isDatabaseInitialized;
    public bool IsDatabaseInitialized
    {
        get => _isDatabaseInitialized;
        set => SetProperty(ref _isDatabaseInitialized, value);
    }

    private string _statusMessage = "Initializing...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private long _totalGames;
    public long TotalGames
    {
        get => _totalGames;
        set => SetProperty(ref _totalGames, value);
    }

    private LibraryMoveStatModel _selectedOpeningMove;
    public LibraryMoveStatModel SelectedOpeningMove
    {
        get => _selectedOpeningMove;
        set
        {
            if (SetProperty(ref _selectedOpeningMove, value) && value != null)
            {
                OnOpeningMoveSelected(value);
            }
        }
    }

    #endregion

    #region Commands

    public DelegateCommand UndoCommand { get; private set; }
    public DelegateCommand ResetCommand { get; private set; }
    public DelegateCommand GoToStartCommand { get; private set; }

    private void InitializeCommands()
    {
        UndoCommand = new DelegateCommand(OnUndo, CanUndo);
        ResetCommand = new DelegateCommand(OnReset, CanReset);
        GoToStartCommand = new DelegateCommand(OnReset, CanReset);
    }

    private bool CanUndo() => MoveItems.Count > 0;
    private bool CanReset() => MoveItems.Count > 0;

    private async void OnUndo()
    {
        if (MoveItems.Count == 0) return;

        MoveItems.RemoveAt(MoveItems.Count - 1);
        _position.UnMake();
        Board.SyncFromPosition();
        Depth = MoveItems.Count;
        MoveHistoryText = BuildMoveHistoryText();

        await UpdateOpeningMovesAsync();
        RaiseCommandsCanExecute();
    }

    private async void OnReset()
    {
        MoveItems.Clear();
        _position.Clear();
        Board.LoadPosition(_position);
        Board.SyncFromPosition();
        Depth = 0;
        CurrentOpeningName = "Starting Position";
        MoveHistoryText = string.Empty;
        IsInBook = true;

        await UpdateOpeningMovesAsync();
        RaiseCommandsCanExecute();
    }

    private void RaiseCommandsCanExecute()
    {
        UndoCommand.RaiseCanExecuteChanged();
        ResetCommand.RaiseCanExecuteChanged();
        GoToStartCommand.RaiseCanExecuteChanged();
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            StatusMessage = "Loading databases...";
            
            // Connect to opening explorer
            await _openingExplorer.ConnectAsync();
            
            // Wait for game database
            _gameDbService.WaitToData();
            
            TotalGames = _gameDbService.GetTotalGames();
            IsDatabaseInitialized = TotalGames > 0;
            
            if (IsDatabaseInitialized)
            {
                StatusMessage = $"{TotalGames:N0} games";
                await UpdateOpeningMovesAsync();
            }
            else
            {
                StatusMessage = "No game data available.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsDatabaseInitialized = false;
        }
    }

    #endregion

    #region Opening Navigation

    private async void OnOpeningMoveSelected(LibraryMoveStatModel moveModel)
    {
        if (moveModel == null) return;

        try
        {
            // Find the move by key
            IEnumerable<MoveBase> legalMoves;
            
            if (MoveItems.Count == 0)
                legalMoves = _position.GetFirstMoves();
            else
                legalMoves = _position.GetAllMoves();

            var move = legalMoves.FirstOrDefault(m => m.Key == moveModel.Key);

            if (move != null)
            {
                // Make the move
                if (MoveItems.Count == 0)
                    _position.MakeFirst(move);
                else
                    _position.Make(move);

                // Add to move history
                int moveNum = (MoveItems.Count / 2) + 1;
                bool isWhite = MoveItems.Count % 2 == 0;
                var notation = _moveFormatter.Format(move);
                
                MoveItems.Add(new LibraryMoveModel
                {
                    Number = MoveItems.Count + 1,
                    MoveNumber = isWhite ? $"{moveNum}." : $"{moveNum}...",
                    Move = notation,
                    IsWhite = isWhite
                });

                Board.SyncFromPosition();
                Depth = MoveItems.Count;
                MoveHistoryText = BuildMoveHistoryText();

                await UpdateOpeningMovesAsync();
                RaiseCommandsCanExecute();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            // Clear selection to allow re-selection of the same item
            _ = Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _selectedOpeningMove = null;
                RaisePropertyChanged(nameof(SelectedOpeningMove));
            });
        }
    }

    private async Task UpdateOpeningMovesAsync()
    {
        OpeningMoves.Clear();

        try
        {
            // Get legal moves from current position
            IEnumerable<MoveBase> legalMoves;
            
            if (MoveItems.Count == 0)
                legalMoves = _position.GetFirstMoves();
            else
                legalMoves = _position.GetAllMoves();

            var movesList = legalMoves.ToList();

            // Get the history key for database lookup
            var historyKey = _moveHistoryService.GetSequence(_searchDepth);
            var history = _gameDbService.Get(historyKey);

            var models = new List<LibraryMoveStatModel>();
            bool isWhiteToMove = _position.GetTurn() == Turn.White;

            foreach (var move in movesList)
            {
                var book = history.GetBookValue(move.Key);
                int total = book.GetTotal();

                var item = new LibraryMoveStatModel
                {
                    Key = move.Key,
                    Move = _moveFormatter.Format(move),
                    Total = total,
                    WhiteWins = book.White,
                    Draws = book.Draw,
                    BlackWins = book.Black,
                    WhitePercent = book.GetWhitePercentage(total),
                    DrawPercent = book.GetDrawPercentage(total),
                    BlackPercent = book.GetBlackPercentage(total),
                    Difference = isWhiteToMove ? book.GetWhite() : book.GetBlack(),
                    Relation = total > 0 
                        ? Math.Round(isWhiteToMove ? 1.0 * book.White / Math.Max(book.Black, 1) : 1.0 * book.Black / Math.Max(book.White, 1), 2)
                        : 0,
                    PercentDiff = book.GetPercentageDifference(total, isWhiteToMove)
                };

                models.Add(item);
            }

            // Sort by total games (most popular first)
            models = models.OrderByDescending(m => m.Total).ToList();

            // Assign ranks and add to collection
            for (int i = 0; i < models.Count; i++)
            {
                models[i].Rank = i + 1;
                OpeningMoves.Add(models[i]);
            }

            // Update opening name - try OpeningExplorerService first, then fall back to LocalDbService
            await UpdateOpeningNameAsync(historyKey);

            // Count moves with game data
            int movesWithData = models.Count(m => m.Total > 0);
            StatusMessage = $"{movesWithData} moves with data";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task UpdateOpeningNameAsync(byte[] historyKey)
    {
        try
        {
            // Build position key from move history for OpeningExplorerService
            var positionKey = GetCurrentPositionKey();
            
            if (!string.IsNullOrEmpty(positionKey) && _openingExplorer.IsInitialized())
            {
                var opening = await _openingExplorer.GetOpeningByPositionAsync(positionKey);
                if (opening != null)
                {
                    CurrentOpeningName = opening.FullName;
                    IsInBook = true;
                    return;
                }
            }

            // Fall back to LocalDbService
            var openingName = _localDbService.GetDebutName(historyKey);
            if (!string.IsNullOrWhiteSpace(openingName))
            {
                CurrentOpeningName = openingName;
                IsInBook = true;
            }
            else if (MoveItems.Count > 0)
            {
                IsInBook = false;
            }
            else
            {
                CurrentOpeningName = "Starting Position";
                IsInBook = true;
            }
        }
        catch
        {
            // Silently fall back to LocalDbService on any error
            var openingName = _localDbService.GetDebutName(historyKey);
            if (!string.IsNullOrWhiteSpace(openingName))
            {
                CurrentOpeningName = openingName;
                IsInBook = true;
            }
        }
    }

    private string GetCurrentPositionKey()
    {
        if (MoveItems.Count == 0) return string.Empty;

        var moves = _position.GetHistory().ToList();
        var uciMoves = moves.Select(m => m.ToUciString()).ToList();
        return string.Join("_", uciMoves);
    }

    private string BuildMoveHistoryText()
    {
        if (MoveItems.Count == 0) return string.Empty;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < MoveItems.Count; i++)
        {
            var item = MoveItems[i];
            if (item.IsWhite)
            {
                if (i > 0) sb.Append(' ');
                sb.Append($"{(i / 2) + 1}.");
            }
            sb.Append($" {item.Move}");
        }
        return sb.ToString().Trim();
    }

    #endregion
}

/// <summary>
/// Model for a played move in the game history.
/// </summary>
public class LibraryMoveModel : BindableBase
{
    public int Number { get; set; }
    public string MoveNumber { get; set; } = string.Empty;
    public string Move { get; set; } = string.Empty;
    public bool IsWhite { get; set; }
}

/// <summary>
/// Model for move statistics from the game database.
/// </summary>
public class LibraryMoveStatModel : BindableBase
{
    public short Key { get; set; }
    
    private int _rank;
    public int Rank
    {
        get => _rank;
        set => SetProperty(ref _rank, value);
    }

    public string Move { get; set; } = string.Empty;
    
    public int Total { get; set; }
    public int WhiteWins { get; set; }
    public int Draws { get; set; }
    public int BlackWins { get; set; }
    
    public double WhitePercent { get; set; }
    public double DrawPercent { get; set; }
    public double BlackPercent { get; set; }
    
    public int Difference { get; set; }
    public double Relation { get; set; }
    public short PercentDiff { get; set; }

    // Display helpers
    public string TotalDisplay => Total.ToString("N0");
    public string WhitePercentDisplay => $"{WhitePercent:F1}%";
    public string DrawPercentDisplay => $"{DrawPercent:F1}%";
    public string BlackPercentDisplay => $"{BlackPercent:F1}%";
    public string DifferenceDisplay => Difference >= 0 ? $"+{Difference}" : Difference.ToString();
    public string RelationDisplay => Relation.ToString("F2");
    public string PercentDiffDisplay => PercentDiff >= 0 ? $"+{PercentDiff}" : PercentDiff.ToString();
    
    public bool HasData => Total > 0;
}
