using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.Core.Services;
using Analysis.DataAccess.Interfaces;
using Analysis.KioUI.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Analysis.KioUI.ViewModels;

public class PlayViewModel : BindableBase, IDisposable
{
    private readonly IMoveFormatter    _moveFormatter;
    private readonly ISoundService     _soundService;
    private readonly ISettingsService  _settingsService;
    private readonly IStockfishService _stockfish;
    private readonly IAnalysisService  _analysisService;
    private readonly IOpeningExplorerService _openingExplorer;
    private readonly MoveHistoryService _moveHistory;
    private readonly AnalyseViewModel _analyseViewModel;

    private readonly Position _position;
    private Turn _playerTurn = Turn.White;
    private EloProfile _currentProfile = EloProfile.Presets[2];
    private CancellationTokenSource _engineCts;

    private readonly DispatcherTimer _clockTimer;
    private TimeSpan _whiteTime = TimeSpan.Zero;
    private TimeSpan _blackTime = TimeSpan.Zero;
    private readonly List<MoveBase> _movesPlayed = [];
    private readonly Stack<MoveBase> _undoneMovesStack = new();

    // Constructor
    public PlayViewModel(
        IMoveFormatter moveFormatter,
        ISoundService soundService,
        ISettingsService settingsService,
        IStockfishService stockfish,
        IAnalysisService analysisService,
        IOpeningExplorerService openingExplorer,
        MoveHistoryService moveHistory,
        AnalyseViewModel analyseViewModel,
        Position position)
    {
        _moveFormatter   = moveFormatter;
        _soundService    = soundService;
        _settingsService = settingsService;
        _stockfish       = stockfish;
        _analysisService = analysisService;
        _openingExplorer = openingExplorer;
        _moveHistory     = moveHistory;
        _analyseViewModel = analyseViewModel;
        _position        = position;

        Board    = new BoardViewModel();
        Board.MoveMade += OnHumanMoveMade;
        MoveList = [];

        NewGameCommand   = new DelegateCommand(OnNewGame);
        UndoCommand      = new DelegateCommand(OnUndo,      CanUndo);
        RedoCommand      = new DelegateCommand(OnRedo,      CanRedo);
        ResignCommand    = new DelegateCommand(OnResign,    CanResign);
        OfferDrawCommand = new DelegateCommand(OnOfferDraw, CanOfferDraw);
        AnalyzeGameCommand = new DelegateCommand(OnAnalyzeGame, CanAnalyzeGame);
        DismissGameOverCommand = new DelegateCommand(OnDismissGameOver);

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += OnClockTick;

        StatusText = "Press \"New Game\" to start.";
    }

    // ?? Child ViewModels ?????????????????????????????????????????
    public BoardViewModel Board { get; }

    // – Commands ––––––––––––––––––––––––––––––––––––––––––––––––––––
    public DelegateCommand NewGameCommand   { get; }
    public DelegateCommand UndoCommand      { get; }
    public DelegateCommand RedoCommand      { get; }
    public DelegateCommand ResignCommand    { get; }
    public DelegateCommand OfferDrawCommand { get; }
    public DelegateCommand AnalyzeGameCommand { get; }
    public DelegateCommand DismissGameOverCommand { get; }

    // ?? Collections ??????????????????????????????????????????????
    public ObservableCollection<MoveListItemViewModel> MoveList { get; }

    // ?? Display properties ???????????????????????????????????????
    private string _playerName = "You";
    public string PlayerName
    {
        get => _playerName;
        set => SetProperty(ref _playerName, value);
    }

    private string _opponentName = "Stockfish";
    public string OpponentName
    {
        get => _opponentName;
        set => SetProperty(ref _opponentName, value);
    }

    private string _playerColorDisplay = "White";
    public string PlayerColorDisplay
    {
        get => _playerColorDisplay;
        set => SetProperty(ref _playerColorDisplay, value);
    }

    private string _opponentEloDisplay = "ELO 1300";
    public string OpponentEloDisplay
    {
        get => _opponentEloDisplay;
        set => SetProperty(ref _opponentEloDisplay, value);
    }

    private string _playerClock = "0:00";
    public string PlayerClock
    {
        get => _playerClock;
        set => SetProperty(ref _playerClock, value);
    }

    private string _opponentClock = "0:00";
    public string OpponentClock
    {
        get => _opponentClock;
        set => SetProperty(ref _opponentClock, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private bool _isEngineThinking;
    public bool IsEngineThinking
    {
        get => _isEngineThinking;
        set => SetProperty(ref _isEngineThinking, value);
    }

    private bool _isGameOver;
    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            if (SetProperty(ref _isGameOver, value))
            {
                ResignCommand.RaiseCanExecuteChanged();
                OfferDrawCommand.RaiseCanExecuteChanged();
                
                // Show overlay when game ends
                if (value)
                    ShowGameOverOverlay = true;
            }
        }
    }

    private bool _showGameOverOverlay;
    /// <summary>
    /// Controls visibility of the game-over overlay.
    /// Can be dismissed to review the board while game is still over.
    /// </summary>
    public bool ShowGameOverOverlay
    {
        get => _showGameOverOverlay;
        set => SetProperty(ref _showGameOverOverlay, value);
    }

    private string _gameResultText = string.Empty;
    public string GameResultText
    {
        get => _gameResultText;
        set => SetProperty(ref _gameResultText, value);
    }

    private string _gameResultDetail = string.Empty;
    public string GameResultDetail
    {
        get => _gameResultDetail;
        set => SetProperty(ref _gameResultDetail, value);
    }

    private string _boardEvaluation = "0.0";
    public string BoardEvaluation
    {
        get => _boardEvaluation;
        set => SetProperty(ref _boardEvaluation, value);
    }

    private string _lastMoveClassification = string.Empty;
    public string LastMoveClassification
    {
        get => _lastMoveClassification;
        set => SetProperty(ref _lastMoveClassification, value);
    }

    private string _currentOpeningName = string.Empty;
    public string CurrentOpeningName
    {
        get => _currentOpeningName;
        set => SetProperty(ref _currentOpeningName, value);
    }

    private string _currentOpeningECO = string.Empty;
    public string CurrentOpeningECO
    {
        get => _currentOpeningECO;
        set => SetProperty(ref _currentOpeningECO, value);
    }

    private string _currentOpeningMoves = string.Empty;
    public string CurrentOpeningMoves
    {
        get => _currentOpeningMoves;
        set => SetProperty(ref _currentOpeningMoves, value);
    }

    private bool _isInOpeningBook;
    public bool IsInOpeningBook
    {
        get => _isInOpeningBook;
        set => SetProperty(ref _isInOpeningBook, value);
    }

    private bool _hasOpeningData;
    /// <summary>
    /// True if we have any opening data to show (even if out of book)
    /// </summary>
    public bool HasOpeningData
    {
        get => _hasOpeningData;
        set => SetProperty(ref _hasOpeningData, value);
    }

    // Player's last move classification
    private MoveClassification _playerMoveClassification = MoveClassification.None;
    public MoveClassification PlayerMoveClassification
    {
        get => _playerMoveClassification;
        set => SetProperty(ref _playerMoveClassification, value);
    }

    private string _playerBestMoveSequence = string.Empty;
    public string PlayerBestMoveSequence
    {
        get => _playerBestMoveSequence;
        set => SetProperty(ref _playerBestMoveSequence, value);
    }

    private bool _hasPlayerBestMoveSequence;
    public bool HasPlayerBestMoveSequence
    {
        get => _hasPlayerBestMoveSequence;
        set => SetProperty(ref _hasPlayerBestMoveSequence, value);
    }

    // Engine's last move classification
    private MoveClassification _engineMoveClassification = MoveClassification.None;
    public MoveClassification EngineMoveClassification
    {
        get => _engineMoveClassification;
        set => SetProperty(ref _engineMoveClassification, value);
    }

    private string _engineBestMoveSequence = string.Empty;
    public string EngineBestMoveSequence
    {
        get => _engineBestMoveSequence;
        set => SetProperty(ref _engineBestMoveSequence, value);
    }

    private bool _hasEngineBestMoveSequence;
    public bool HasEngineBestMoveSequence
    {
        get => _hasEngineBestMoveSequence;
        set => SetProperty(ref _hasEngineBestMoveSequence, value);
    }

    // ?? New Game ?????????????????????????????????????????????????
    private void OnNewGame()
    {
        var dlg = new Views.NewGameDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;

        var dialogVm = (NewGameDialogViewModel)dlg.DataContext;
        if (!dialogVm.Confirmed) return;

        _currentProfile    = dialogVm.SelectedProfile;
        _playerTurn        = dialogVm.PlayAsWhite ? Turn.White : Turn.Black;
        PlayerColorDisplay = _playerTurn == Turn.White ? "White" : "Black";
        OpponentEloDisplay = _currentProfile.DisplayLabel;
        OpponentName       = $"Stockfish ({_currentProfile.Name})";
        Board.IsFlipped    = _playerTurn == Turn.Black;

        _engineCts?.Cancel();
        _engineCts?.Dispose();
        _engineCts = null;

        // Rewind the shared MoveHistoryService back to _ply = -1 by unmaking
        // every move of the previous game. Without this the history depth counter
        // keeps growing across games and eventually overflows the fixed-size arrays,
        // causing IndexOutOfRangeException inside the engine.
        _position.Clear();

        _movesPlayed.Clear();
        _undoneMovesStack.Clear();
        MoveList.Clear();

        _whiteTime    = TimeSpan.Zero;
        _blackTime    = TimeSpan.Zero;
        PlayerClock   = "0:00";
        OpponentClock = "0:00";

        IsGameOver       = false;
        ShowGameOverOverlay = false;
        IsEngineThinking = false;
        GameResultText   = string.Empty;
        GameResultDetail = string.Empty;
        
        // Clear player classification
        PlayerMoveClassification = MoveClassification.None;
        PlayerBestMoveSequence = string.Empty;
        HasPlayerBestMoveSequence = false;
        
        // Clear engine classification
        EngineMoveClassification = MoveClassification.None;
        EngineBestMoveSequence = string.Empty;
        HasEngineBestMoveSequence = false;
        
        // Reset opening display
        CurrentOpeningName = string.Empty;
        CurrentOpeningECO = string.Empty;
        CurrentOpeningMoves = string.Empty;
        IsInOpeningBook = false;
        HasOpeningData = false;

        Board.LoadPosition(_position);
        _clockTimer.Start();

        if (_playerTurn == Turn.Black)
        {
            Board.BoardState = BoardState.EngineThinking;
            IsEngineThinking = true;
            StatusText = "Engine is making the first move...";
            _ = InitialiseAndEngineFirstMoveAsync();
        }
        else
        {
            Board.BoardState = BoardState.Interactive;
            StatusText = "Your turn (White)";
            _ = InitialiseStockfishAsync();  // await in background; IsReady will be true well before move 1 response needed
        }

        _soundService.PlayClick();
        UndoCommand.RaiseCanExecuteChanged();
    }

    private async Task InitialiseAndEngineFirstMoveAsync()
    {
        await InitialiseStockfishAsync();
        await RequestEngineMoveAsync();
    }

    private async Task InitialiseStockfishAsync()
    {
        try
        {
            var path = _settingsService.Current.StockfishPath;
            if (!System.IO.File.Exists(path))
            {
                Application.Current.Dispatcher.Invoke(
                    () => StatusText = "Stockfish not found. Update path in Settings.");
                return;
            }
            
            if (!_stockfish.IsReady)
            {
                await _stockfish.InitialiseAsync(path);
                
                // Configure with optimized settings for gameplay + evaluation
                await _stockfish.ConfigureAsync(new StockfishOptions
                {
                    HashSizeMB = 128,
                    Threads = 1,
                    MultiPV = 1,
                    UsePositionHistory = true,
                    EvaluationDepth = 15
                });
            }
            
            // Clear history for new game
            await _stockfish.ClearHistoryAsync();
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(
                () => StatusText = $"Stockfish error: {ex.Message}");
        }
    }

    // -- Human move --------------------------------------------------------
    private void OnHumanMoveMade(MoveBase move)
    {
        // Clear redo stack when new move is made
        _undoneMovesStack.Clear();
        RedoCommand.RaiseCanExecuteChanged();
        
        // Execute the move atomically through Position: updates board, history,
        // turn, check flag and board-history in one call.
        if (_moveHistory.GetPly() < 0)
            _position.MakeFirst(move);  // game's very first move
        else
            _position.Make(move);       // all subsequent moves

        Board.SyncFromPosition();
        Board.SetLastMoveHighlightPublic(move.From, move.To);

        PlayMoveSound(move);
        RecordMove(move);

        // Detect and update opening
        _ = DetectCurrentOpeningAsync();

        // Evaluate player's move and show best sequence if not optimal
        _ = EvaluatePlayerMoveAsync(move);

        var result = EvaluateGameResult();
        if (result != GameResult.Continue) { EndGame(result); return; }

        Board.BoardState = BoardState.EngineThinking;
        IsEngineThinking = true;
        StatusText = "Engine thinking...";
        _ = RequestEngineMoveAsync();
    }

    private async Task EvaluatePlayerMoveAsync(MoveBase move)
    {
        try
        {
            if (!_stockfish.IsReady) return;
            
            // Get position before the move
            string movesBefore = _movesPlayed.Count > 1 
                ? UciMoveConverter.BuildMoveSequence(_movesPlayed.Take(_movesPlayed.Count - 1).ToList())
                : string.Empty;
            
            string playedMove = UciMoveConverter.ToUci(move);
            
            // Build position key for opening book lookup (all moves including this one)
            var allMoves = _movesPlayed.Select(m => UciMoveConverter.ToUci(m));
            string positionKey = string.Join("_", allMoves);
            
            // Check if this move is in the opening book FIRST
            bool isBookMove = false;
            try
            {
                var opening = await _openingExplorer.GetOpeningByPositionAsync(positionKey);
                isBookMove = opening != null;
            }
            catch
            {
                // If opening lookup fails, continue with engine evaluation
                isBookMove = false;
            }
            
            MoveEvaluation evaluation;
            
            if (isBookMove)
            {
                // Book move - create a Book classification without engine evaluation
                evaluation = new MoveEvaluation
                {
                    EvalBefore = 0,
                    EvalAfter = 0,
                    BestMoveEval = 0,
                    BestMove = string.Empty,
                    PlayedMove = playedMove,
                    BestMoveSequence = string.Empty,
                    Classification = MoveClassification.Book,
                };
            }
            else
            {
                // Evaluate the move with engine (background task)
                evaluation = await _stockfish.EvaluateMoveAsync(movesBefore, playedMove, depth: 12);
            }
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update PLAYER classification (just the enum - converters handle the rest)
                PlayerMoveClassification = evaluation.Classification;
                
                // Show best move sequence for non-optimal moves
                if (!string.IsNullOrEmpty(evaluation.BestMoveSequence))
                {
                    var moves = evaluation.BestMoveSequence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var sequence = string.Join(" ", moves.Take(Math.Min(4, moves.Length)));
                    PlayerBestMoveSequence = sequence;
                    
                    // Show for all moves except Best, Brilliant, and Book
                    HasPlayerBestMoveSequence = evaluation.Classification != MoveClassification.Best && 
                                                evaluation.Classification != MoveClassification.Brilliant &&
                                                evaluation.Classification != MoveClassification.Book;
                }
                else
                {
                    PlayerBestMoveSequence = string.Empty;
                    HasPlayerBestMoveSequence = false;
                }
                
                // Update board evaluation
                BoardEvaluation = evaluation.EvalAfter >= 0 
                    ? $"+{evaluation.EvalAfter / 100.0:F2}" 
                    : $"{evaluation.EvalAfter / 100.0:F2}";
            });
        }
        catch (Exception)
        {
            // Silently fail - don't interrupt gameplay
        }
    }

    private async Task EvaluateEngineMoveAsync(MoveBase move)
    {
        try
        {
            if (!_stockfish.IsReady) return;
            
            // Get position before the move
            string movesBefore = _movesPlayed.Count > 1 
                ? UciMoveConverter.BuildMoveSequence(_movesPlayed.Take(_movesPlayed.Count - 1).ToList())
                : string.Empty;
            
            string playedMove = UciMoveConverter.ToUci(move);
            
            // Build position key for opening book lookup (all moves including this one)
            var allMoves = _movesPlayed.Select(m => UciMoveConverter.ToUci(m));
            string positionKey = string.Join("_", allMoves);
            
            // Check if this move is in the opening book FIRST
            bool isBookMove = false;
            try
            {
                var opening = await _openingExplorer.GetOpeningByPositionAsync(positionKey);
                isBookMove = opening != null;
            }
            catch
            {
                // If opening lookup fails, continue with engine evaluation
                isBookMove = false;
            }
            
            MoveEvaluation evaluation;
            
            if (isBookMove)
            {
                // Book move - create a Book classification without engine evaluation
                evaluation = new MoveEvaluation
                {
                    EvalBefore = 0,
                    EvalAfter = 0,
                    BestMoveEval = 0,
                    BestMove = string.Empty,
                    PlayedMove = playedMove,
                    BestMoveSequence = string.Empty,
                    Classification = MoveClassification.Book,
                };
            }
            else
            {
                // Evaluate the move with engine (background task)
                evaluation = await _stockfish.EvaluateMoveAsync(movesBefore, playedMove, depth: 12);
            }
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update ENGINE classification (just the enum - converters handle the rest)
                EngineMoveClassification = evaluation.Classification;
                
                // Show best move sequence only if engine made a mistake (for educational purposes)
                if (!string.IsNullOrEmpty(evaluation.BestMoveSequence) &&
                    (evaluation.Classification == MoveClassification.Inaccuracy ||
                     evaluation.Classification == MoveClassification.Mistake ||
                     evaluation.Classification == MoveClassification.Blunder))
                {
                    var moves = evaluation.BestMoveSequence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var sequence = string.Join(" ", moves.Take(Math.Min(4, moves.Length)));
                    EngineBestMoveSequence = sequence;
                    HasEngineBestMoveSequence = true;
                }
                else
                {
                    EngineBestMoveSequence = string.Empty;
                    HasEngineBestMoveSequence = false;
                }
                
                // Update board evaluation
                BoardEvaluation = evaluation.EvalAfter >= 0 
                    ? $"+{evaluation.EvalAfter / 100.0:F2}" 
                    : $"{evaluation.EvalAfter / 100.0:F2}";
            });
        }
        catch (Exception)
        {
            // Silently fail - don't interrupt gameplay
        }
    }

    // ?? Engine move ??????????????????????????????????????????????
    private async Task RequestEngineMoveAsync()
    {
        _engineCts?.Cancel();
        _engineCts?.Dispose();
        _engineCts = new CancellationTokenSource();
        var ct = _engineCts.Token;

        // Capture everything that touches _position on the UI thread BEFORE going async.
        // GetAllMoves() mutates internal engine state; calling it on a background thread
        // while the UI thread can also mutate _position causes "index out of bounds" crashes.
        string uciSeq    = UciMoveConverter.BuildMoveSequence(_movesPlayed);
        var    legalMoves = _position.GetAllMoves();

        try
        {
            string bestUci = _stockfish.IsReady
                ? await _stockfish.GetBestMoveAsync(uciSeq, _currentProfile, ct)
                : GetRandomLegalUci(legalMoves);

            if (ct.IsCancellationRequested || string.IsNullOrEmpty(bestUci) || bestUci == "(none)")
                return;

            var engineMove = UciMoveConverter.FromUci(bestUci, legalMoves);
            if (engineMove is null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ct.IsCancellationRequested) return;

                // Execute atomically: board + history + turn + check flag.
                if (_moveHistory.GetPly() < 0)
                    _position.MakeFirst(engineMove);
                else
                    _position.Make(engineMove);

                Board.SyncFromPosition();
                Board.SetLastMoveHighlightPublic(engineMove.From, engineMove.To);

                PlayMoveSound(engineMove);
                RecordMove(engineMove);

                // Detect and update opening
                _ = DetectCurrentOpeningAsync();
                
                // Evaluate engine's move and display classification
                _ = EvaluateEngineMoveAsync(engineMove);

                IsEngineThinking = false;

                var result = EvaluateGameResult();
                if (result != GameResult.Continue) { EndGame(result); return; }

                Board.BoardState = BoardState.Interactive;
                var label = _playerTurn == Turn.White ? "White" : "Black";
                StatusText = $"Your turn ({label})";
                UndoCommand.RaiseCanExecuteChanged();
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsEngineThinking = false;
                Board.BoardState = BoardState.Interactive;
                StatusText = $"Engine error: {ex.Message}";
            });
        }
    }

    private string GetRandomLegalUci(IList<MoveBase> legalMoves)
    {
        if (legalMoves.Count == 0) return string.Empty;
        return UciMoveConverter.ToUci(legalMoves[Random.Shared.Next(legalMoves.Count)]);
    }

    // ?? Undo ?????????????????????????????????????????????????????
    private bool CanUndo() => _movesPlayed.Count > 0;  // Allow undo even after game over

    private void OnUndo()
    {
        if (!CanUndo()) return;

        _engineCts?.Cancel();
        IsEngineThinking = false;

        int undoCount = Math.Min(2, _movesPlayed.Count);
        for (int i = 0; i < undoCount; i++)
        {
            var move = _movesPlayed[^1];
            _undoneMovesStack.Push(move);  // Save for redo
            _movesPlayed.RemoveAt(_movesPlayed.Count - 1);
            _position.UnMake(); // decrements MoveHistoryService._ply via Remove()
        }

        Board.SyncFromPosition();

        // Rebuild move list to properly reflect undone moves
        RebuildMoveList();

        Board.BoardState = BoardState.Interactive;
        StatusText = "Move undone — your turn.";
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
        
        // Clear player classification
        PlayerMoveClassification = MoveClassification.None;
        PlayerBestMoveSequence = string.Empty;
        HasPlayerBestMoveSequence = false;
        
        // Clear engine classification
        EngineMoveClassification = MoveClassification.None;
        EngineBestMoveSequence = string.Empty;
        HasEngineBestMoveSequence = false;
        
        // Update opening for current position after undo
        _ = DetectCurrentOpeningAsync();
    }

    // ?? Redo ?????????????????????????????????????????????????????
    private bool CanRedo() => _undoneMovesStack.Count > 0 && !IsGameOver;

    private void OnRedo()
    {
        if (!CanRedo()) return;

        _engineCts?.Cancel();
        IsEngineThinking = false;

        int redoCount = Math.Min(2, _undoneMovesStack.Count);
        for (int i = 0; i < redoCount; i++)
        {
            var move = _undoneMovesStack.Pop();
            _movesPlayed.Add(move);
            
            if (_moveHistory.GetPly() < 0)
                _position.MakeFirst(move);
            else
                _position.Make(move);
        }

        Board.SyncFromPosition();
        
        // Rebuild move list display
        RebuildMoveList();

        Board.BoardState = BoardState.Interactive;
        StatusText = "Move redone — your turn.";
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
        
        // Update opening for current position after redo
        _ = DetectCurrentOpeningAsync();
    }

    private void RebuildMoveList()
    {
        MoveList.Clear();
        for (int i = 0; i < _movesPlayed.Count; i++)
        {
            int moveNum = (i / 2) + 1;
            bool isWhite = i % 2 == 0;
            string notation = _moveFormatter.Format(_movesPlayed[i]);

            if (isWhite)
            {
                MoveList.Add(new MoveListItemViewModel
                {
                    Number = moveNum,
                    White = notation
                });
            }
            else
            {
                if (MoveList.Count > 0)
                    MoveList[^1].Black = notation;
            }
        }
    }

    // – Resign / Draw / Analyze –––––––––––––––––––––––––––––––––––––
    private bool CanResign() => !IsGameOver && _movesPlayed.Count > 0;
    
    private void OnResign()
    {
        _engineCts?.Cancel();
        _clockTimer.Stop();
        IsGameOver       = true;
        Board.BoardState = BoardState.ReadOnly;
        GameResultText   = "You Resigned";
        GameResultDetail = "Engine wins";
        _soundService.PlayLose();
        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
        AnalyzeGameCommand.RaiseCanExecuteChanged();
    }

    private bool CanOfferDraw() => !IsGameOver && _movesPlayed.Count > 0;
    
    private void OnOfferDraw()
    {
        _clockTimer.Stop();
        IsGameOver       = true;
        Board.BoardState = BoardState.ReadOnly;
        GameResultText   = "Draw";
        GameResultDetail = "1/2 - 1/2";
        _soundService.PlayDraw();
        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
        AnalyzeGameCommand.RaiseCanExecuteChanged();
    }

    private bool CanAnalyzeGame() => _movesPlayed.Count > 0;
    
    private void OnAnalyzeGame()
    {
        if (_movesPlayed.Count == 0) return;

        // Navigate to Analyze tab and load current game
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Find the TabControl in MainWindow
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow?.Content is Grid grid)
            {
                var tabControl = FindVisualChild<TabControl>(grid);
                if (tabControl != null)
                {
                    // Switch to Analyze tab (index 1)
                    tabControl.SelectedIndex = 1;
                    
                    // Load the game into AnalyseViewModel
                    _analyseViewModel.LoadGame(_movesPlayed.ToList());
                    StatusText = "Game loaded in Analysis tab";
                }
            }
        });
    }

    private void OnDismissGameOver()
    {
        // Hide the overlay so player can review the board
        ShowGameOverOverlay = false;
    }

    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
                return typedChild;
            
            var result = FindVisualChild<T>(child);
            if (result != null)
                return result;
        }
        return null;
    }

    // ?? Game result detection ????????????????????????????????????
    private GameResult EvaluateGameResult()
    {
        if (_moveHistory.IsThreefoldRepetition()) return GameResult.ThreefoldRepetition;
        if (_moveHistory.IsFiftyMoves())          return GameResult.FiftyMoves;

        var moves = _position.GetAllMoves();
        if (moves.Count == 0)
            return _moveHistory.IsLastMoveWasCheck() ? GameResult.Mate : GameResult.Pat;

        return GameResult.Continue;
    }

    private void EndGame(GameResult result)
    {
        _clockTimer.Stop();
        IsGameOver       = true;
        Board.BoardState = BoardState.ReadOnly;
        _engineCts?.Cancel();

        var currentTurn = _position.GetTurn();

        (GameResultText, GameResultDetail) = result switch
        {
            GameResult.Mate when currentTurn != _playerTurn => ("You Win!",    "Checkmate"),
            GameResult.Mate                                  => ("You Lose",    "Checkmate"),
            GameResult.Pat                                   => ("Draw",        "Stalemate"),
            GameResult.ThreefoldRepetition                   => ("Draw",        "Threefold repetition"),
            GameResult.FiftyMoves                            => ("Draw",        "Fifty-move rule"),
            _                                                => ("Game Over",   string.Empty)
        };

        if (result == GameResult.Mate && currentTurn != _playerTurn) _soundService.PlayWin();
        else if (result == GameResult.Mate)                          _soundService.PlayLose();
        else                                                         _soundService.PlayDraw();

        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
        AnalyzeGameCommand.RaiseCanExecuteChanged();
    }

    // – Move recording –––––––––––––––––––––––––––––––––––––––––––––––
    private void RecordMove(MoveBase move)
    {
        _movesPlayed.Add(move);
        var notation = _moveFormatter.Format(move);
        int fullMove = (_movesPlayed.Count + 1) / 2;

        if (move.IsWhite)
            MoveList.Add(new MoveListItemViewModel { Number = fullMove, White = notation });
        else if (MoveList.Count > 0)
            MoveList[^1].Black = notation;
        
        // Update command states
        AnalyzeGameCommand.RaiseCanExecuteChanged();
        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
    }

    // – Opening Detection ––––––––––––––––––––––––––––––––––––––––––––
    private async Task DetectCurrentOpeningAsync()
    {
        try
        {
            if (_movesPlayed.Count == 0)
            {
                CurrentOpeningName = string.Empty;
                CurrentOpeningECO = string.Empty;
                CurrentOpeningMoves = string.Empty;
                IsInOpeningBook = false;
                HasOpeningData = false;
                return;
            }

            // Build position key from moves played
            var uciMoves = _movesPlayed.Select(m => UciMoveConverter.ToUci(m));
            var positionKey = string.Join("_", uciMoves);

            // Look up opening in database
            var opening = await _openingExplorer.GetOpeningByPositionAsync(positionKey);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (opening != null)
                {
                    // Found exact opening - update everything
                    CurrentOpeningName = opening.FullName;
                    CurrentOpeningECO = opening.ECO;
                    CurrentOpeningMoves = opening.MovesSAN;
                    IsInOpeningBook = true;
                    HasOpeningData = true;
                }
                else
                {
                    // Out of book - keep last known opening but mark as out of book
                    IsInOpeningBook = false;
                    // HasOpeningData stays true if we had an opening before
                    // CurrentOpeningName, ECO, and Moves remain unchanged (showing last known opening)
                }
            });
        }
        catch (Exception)
        {
            // Silently fail - don't interrupt gameplay
        }
    }

    // ?? Sound helpers ????????????????????????????????????????????
    private void PlayMoveSound(MoveBase move)
    {
        // Sound priority: Promotion > Castle > Check > Capture > Move
        // Promotion takes precedence as it's a special transformation
        if (move.IsPromotion) { _soundService.PlayPromotion(); return; }
        if (move.IsCastle)    { _soundService.PlayCastle();    return; }
        if (move.IsCheck)     { _soundService.PlayCheck();     return; }
        if (move.IsAttack)    { _soundService.PlayCapture();   return; }
        _soundService.PlayMove();
    }

    // ?? Clock ????????????????????????????????????????????????????
    private void OnClockTick(object sender, EventArgs e)
    {
        if (IsGameOver) return;
        var turn = _position.GetTurn();
        if (turn == _playerTurn)
        {
            _whiteTime  = _whiteTime.Add(TimeSpan.FromSeconds(1));
            PlayerClock = FormatClock(_whiteTime);
        }
        else
        {
            _blackTime   = _blackTime.Add(TimeSpan.FromSeconds(1));
            OpponentClock = FormatClock(_blackTime);
        }
    }

    private static string FormatClock(TimeSpan t)
        => t.TotalHours >= 1
            ? $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}"
            : $"{t.Minutes}:{t.Seconds:D2}";

    // ?? IDisposable ??????????????????????????????????????????????
    public void Dispose()
    {
        _clockTimer.Stop();
        _engineCts?.Cancel();
        _engineCts?.Dispose();
        _stockfish.Dispose();
    }
}
