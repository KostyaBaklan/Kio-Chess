using Analysis.Core.Interfaces;
using Analysis.DataAccess.Interfaces;
using Analysis.UI.Common.Models;
using Analysis.UI.Common.ViewModels;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Hash;
using Engine.Models.Moves;
using Engine.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Analysis.Kio.Library.ViewModels;

/// <summary>
/// ViewModel for the Library application.
/// Provides opening exploration with move statistics using the IGameHistoryService.
/// </summary>
public class LibraryViewModel : BindableBase
{
    private readonly IGamesService _gameDbService;
    private readonly IGameHistoryService _gameHistory;
    private readonly IOpeningExplorerService _openingExplorer;
    private readonly IMoveFormatter _moveFormatter;
    private readonly MoveHistoryService _moveHistoryService;
    private readonly ISettingsService _settingsService;
    private readonly short _searchDepth;
    private readonly Position _position;

    public LibraryViewModel(
        IGamesService gameDbService,
        IGameHistoryService gameHistory,
        IOpeningExplorerService openingExplorer,
        IMoveFormatter moveFormatter,
        MoveHistoryService moveHistoryService,
        ISettingsService settingsService,
        IConfigurationProvider configurationProvider,
        Position position)
    {
        _gameDbService = gameDbService;
        _gameHistory = gameHistory;
        _openingExplorer = openingExplorer;
        _moveFormatter = moveFormatter;
        _moveHistoryService = moveHistoryService;
        _settingsService = settingsService;
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
        try
        {
            IEnumerable<MoveBase> legalMoves = MoveItems.Count == 0
                ? _position.GetFirstMoves()
                : _position.GetAllMoves();

            var move = legalMoves.FirstOrDefault(m => m.Key == moveModel.Key);

            if (move != null)
            {
                if (MoveItems.Count == 0)
                    _position.MakeFirst(move);
                else
                    _position.Make(move);

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
            IEnumerable<MoveBase> legalMoves = MoveItems.Count == 0
                ? _position.GetFirstMoves()
                : _position.GetAllMoves();

            var movesList = legalMoves.ToList();

            var sequence = _moveHistoryService.GetSaveSequence();
            var historyHash = MoveHashSequenceHasher.ComputeSequenceHash(sequence);
            var history = _gameHistory.Get(historyHash);

            var models = new List<LibraryMoveStatModel>();
            bool isWhiteToMove = _position.GetTurn() == Turn.White;
            List<short> moveKeys = new List<short>();

            for (int i = 0; i < movesList.Count; i++)
            {
                MoveBase move = movesList[i];
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
                    PercentDiff = book.GetPercentageDifference(total, isWhiteToMove)
                };

                models.Add(item);
                moveKeys.Add(move.Key);
            }

            models = models.OrderByDescending(m => m.Total).ToList();

            for (int i = 0; i < models.Count; i++)
            {
                models[i].Rank = i + 1;
                OpeningMoves.Add(models[i]);
            }

            await UpdateOpeningNameAsync(sequence.ToArray());

            int movesWithData = models.Count(m => m.Total > 0);
            StatusMessage = $"{movesWithData} moves with data";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task UpdateOpeningNameAsync(short[] moveKeys)
    {
        try
        {
            var opening = await _openingExplorer.GetOpeningByMoveKeysAsync(moveKeys);
            if (opening != null)
            {
                CurrentOpeningName = opening.FullName;
                IsInBook = true;
            }
            else
            {
                IsInBook = false;
            }
        }
        catch
        {
        }
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
