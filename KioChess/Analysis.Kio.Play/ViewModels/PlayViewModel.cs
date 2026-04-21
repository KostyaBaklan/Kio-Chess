using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.Core.Services;
using Analysis.DataAccess.Interfaces;
using Analysis.Kio.Play.Views;
using Analysis.UI.Common.Models;
using Analysis.UI.Common.ViewModels;
using Analysis.UI.Common.Views;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Analysis.Kio.Play.ViewModels;

public class PlayViewModel : BindableBase, IDisposable
{
    private readonly IMoveFormatter _moveFormatter;
    private readonly ISoundService _soundService;
    private readonly ISettingsService _settingsService;
    private readonly IStockfishService _stockfish;
    private readonly IOpeningExplorerService _openingExplorer;
    private readonly Analysis.UI.Common.Services.IDialogService _dialogService;
    private readonly MoveHistoryService _moveHistory;

    private readonly Position _position;
    private Turn _playerTurn = Turn.White;
    private EloProfile _currentProfile = EloProfile.Presets[5];
    private CancellationTokenSource _engineCts;

    private readonly DispatcherTimer _clockTimer;
    private TimeSpan _whiteTime = TimeSpan.Zero;
    private TimeSpan _blackTime = TimeSpan.Zero;
    private readonly List<MoveBase> _movesPlayed = [];
    private readonly Stack<MoveBase> _undoneMovesStack = new();

    public PlayViewModel(
        IMoveFormatter moveFormatter,
        ISoundService soundService,
        ISettingsService settingsService,
        IStockfishService stockfish,
        IOpeningExplorerService openingExplorer,
        Analysis.UI.Common.Services.IDialogService dialogService,
        MoveHistoryService moveHistory,
        Position position)
    {
        _moveFormatter = moveFormatter;
        _soundService = soundService;
        _settingsService = settingsService;
        _stockfish = stockfish;
        _openingExplorer = openingExplorer;
        _dialogService = dialogService;
        _moveHistory = moveHistory;
        _position = position;

        Board = new BoardViewModel();
        Board.LoadPosition(_position);
        Board.MoveMade += OnHumanMoveMade;
        MoveList = [];

        NewGameCommand = new DelegateCommand(OnNewGame);
        UndoCommand = new DelegateCommand(OnUndo, CanUndo);
        RedoCommand = new DelegateCommand(OnRedo, CanRedo);
        ResignCommand = new DelegateCommand(OnResign, CanResign);
        OfferDrawCommand = new DelegateCommand(OnOfferDraw, CanOfferDraw);
        AnalyzeGameCommand = new DelegateCommand(OnAnalyzeGame, CanAnalyzeGame);
        DismissGameOverCommand = new DelegateCommand(OnDismissGameOver);
        LoadPgnCommand = new DelegateCommand(OnLoadPgn);
        LoadFenCommand = new DelegateCommand(OnLoadFen);
        PastePgnCommand = new DelegateCommand(OnPastePgn);
        SavePgnCommand = new DelegateCommand(OnSavePgn, CanSavePgn);
        CopyPgnCommand = new DelegateCommand(OnCopyPgn, CanCopyPgn);
        CopyFenCommand = new DelegateCommand(OnCopyFen);
        OpenLibraryCommand = new DelegateCommand(OnOpenLibrary);
        OpenOpeningExplorerCommand = new DelegateCommand(OnOpenOpeningExplorer);

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += OnClockTick;

        StatusText = "Press \"New Game\" to start.";
    }

    public BoardViewModel Board { get; }
    public ObservableCollection<MoveListItemModel> MoveList { get; }

    public DelegateCommand NewGameCommand { get; }
    public DelegateCommand UndoCommand { get; }
    public DelegateCommand RedoCommand { get; }
    public DelegateCommand ResignCommand { get; }
    public DelegateCommand OfferDrawCommand { get; }
    public DelegateCommand AnalyzeGameCommand { get; }
    public DelegateCommand DismissGameOverCommand { get; }
    public DelegateCommand LoadPgnCommand { get; }
    public DelegateCommand LoadFenCommand { get; }
    public DelegateCommand PastePgnCommand { get; }
    public DelegateCommand SavePgnCommand { get; }
    public DelegateCommand CopyPgnCommand { get; }
    public DelegateCommand CopyFenCommand { get; }
    public DelegateCommand OpenLibraryCommand { get; }
    public DelegateCommand OpenOpeningExplorerCommand { get; }

    #region Properties

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
                AnalyzeGameCommand.RaiseCanExecuteChanged();
                if (value) ShowGameOverOverlay = true;
            }
        }
    }

    private bool _showGameOverOverlay;
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

    // Opening data
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

    private bool _hasOpeningData;
    public bool HasOpeningData
    {
        get => _hasOpeningData;
        set => SetProperty(ref _hasOpeningData, value);
    }

    private bool _isOutOfBook;
    public bool IsOutOfBook
    {
        get => _isOutOfBook;
        set => SetProperty(ref _isOutOfBook, value);
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

    #endregion

    #region Game Logic

    private void OnNewGame()
    {
        // Show New Game dialog
        var dlg = new NewGameDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;

        var dialogVm = (NewGameDialogViewModel)dlg.DataContext;
        if (!dialogVm.Confirmed) return;

        // Start local engine if requested
        if (dialogVm.PlayAgainstLocalEngine)
        {
            StartLocalEngine();
        }

        _currentProfile = dialogVm.SelectedProfile;
        _playerTurn = dialogVm.PlayAsWhite ? Turn.White : Turn.Black;
        PlayerColorDisplay = _playerTurn == Turn.White ? "White" : "Black";
        OpponentEloDisplay = _currentProfile.DisplayLabel;
        OpponentName = $"Stockfish ({_currentProfile.Name})";
        Board.IsFlipped = _playerTurn == Turn.Black;

        _engineCts?.Cancel();
        _engineCts?.Dispose();
        _engineCts = null;

        _position.Clear();
        _movesPlayed.Clear();
        _undoneMovesStack.Clear();
        MoveList.Clear();

        _whiteTime = TimeSpan.Zero;
        _blackTime = TimeSpan.Zero;
        PlayerClock = "0:00";
        OpponentClock = "0:00";

        IsGameOver = false;
        ShowGameOverOverlay = false;
        IsEngineThinking = false;
        GameResultText = string.Empty;
        GameResultDetail = string.Empty;
        BoardEvaluation = "0.0";

        // Clear move classifications
        PlayerMoveClassification = MoveClassification.None;
        PlayerBestMoveSequence = string.Empty;
        HasPlayerBestMoveSequence = false;

        EngineMoveClassification = MoveClassification.None;

        // Clear opening data
        CurrentOpeningName = string.Empty;
        CurrentOpeningECO = string.Empty;
        CurrentOpeningMoves = string.Empty;
        HasOpeningData = false;
        IsOutOfBook = false;

        Board.LoadPosition(_position);
        _clockTimer.Start();

        if (_playerTurn == Turn.Black)
        {
            Board.BoardState = BoardState.ReadOnly;
            IsEngineThinking = true;
            StatusText = "Engine is making the first move...";
            _ = InitialiseAndEngineFirstMoveAsync();
        }
        else
        {
            Board.BoardState = BoardState.Interactive;
            StatusText = "Your turn (White)";
            _ = InitialiseStockfishAsync();
        }

        _soundService.PlayClick();
        UndoCommand.RaiseCanExecuteChanged();
        SavePgnCommand.RaiseCanExecuteChanged();
        CopyPgnCommand.RaiseCanExecuteChanged();
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
                await _stockfish.ConfigureAsync(new StockfishOptions
                {
                    HashSizeMB = 128,
                    Threads = 1,
                    MultiPV = 1,
                    UsePositionHistory = true
                });
            }

            await _stockfish.ClearHistoryAsync();
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(
                () => StatusText = $"Stockfish error: {ex.Message}");
        }
    }


    private void OnHumanMoveMade(MoveBase move)
    {
        _undoneMovesStack.Clear();
        RedoCommand.RaiseCanExecuteChanged();

        if (_moveHistory.GetPly() < 0)
            _position.MakeFirst(move);
        else
            _position.Make(move);

        Board.SyncFromPosition();
        Board.SetLastMoveHighlightPublic(move.From, move.To);

        PlayMoveSound(move);
        RecordMove(move);

        // Update command states after first move
        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
        AnalyzeGameCommand.RaiseCanExecuteChanged();

        // Detect opening and evaluate player's move
        _ = DetectCurrentOpeningAsync();
        _ = EvaluatePlayerMoveAsync(move);

        var result = EvaluateGameResult();
        if (result != GameResult.Continue) { EndGame(result); return; }

        Board.BoardState = BoardState.ReadOnly;
        IsEngineThinking = true;
        StatusText = "Engine thinking...";
        _ = RequestEngineMoveAsync();
    }

    private async Task RequestEngineMoveAsync()
    {
        _engineCts?.Cancel();
        _engineCts?.Dispose();
        _engineCts = new CancellationTokenSource();
        var ct = _engineCts.Token;

        string uciSeq = UciMoveConverter.BuildMoveSequence(_movesPlayed);
        var legalMoves = _position.GetAllMoves();

        try
        {
            string bestUci = _stockfish.IsReady
                ? await _stockfish.GetBestMoveAsync(uciSeq, _currentProfile, ct)
                : GetRandomLegalUci(legalMoves);

            if (ct.IsCancellationRequested || string.IsNullOrEmpty(bestUci) || bestUci == "(none)")
                return;

            var engineMove = UciMoveConverter.FromUci(bestUci, legalMoves);
            if (engineMove == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ct.IsCancellationRequested) return;

                if (_moveHistory.GetPly() < 0)
                    _position.MakeFirst(engineMove);
                else
                    _position.Make(engineMove);

                Board.SyncFromPosition();
                Board.SetLastMoveHighlightPublic(engineMove.From, engineMove.To);

                PlayMoveSound(engineMove);
                RecordMove(engineMove);

                // Detect opening and evaluate engine's move
                _ = DetectCurrentOpeningAsync();
                _ = EvaluateEngineMoveAsync(engineMove);

                IsEngineThinking = false;

                var result = EvaluateGameResult();
                if (result != GameResult.Continue) { EndGame(result); return; }

                Board.BoardState = BoardState.Interactive;
                StatusText = $"Your turn ({(_playerTurn == Turn.White ? "White" : "Black")})";
                UndoCommand.RaiseCanExecuteChanged();
                AnalyzeGameCommand.RaiseCanExecuteChanged();
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

    private void PlayMoveSound(MoveBase move)
    {
        if (move.IsCastle)
            _soundService.PlayCastle();
        else
            _soundService.PlayMove();
    }

    private void RecordMove(MoveBase move)
    {
        _movesPlayed.Add(move);
        int moveNum = (_movesPlayed.Count + 1) / 2;
        bool isWhite = _movesPlayed.Count % 2 == 1;
        string notation = _moveFormatter.Format(move);

        if (isWhite)
        {
            MoveList.Add(new MoveListItemModel { Number = moveNum, White = notation });
        }
        else if (MoveList.Count > 0)
        {
            MoveList[^1].Black = notation;
        }

        SavePgnCommand.RaiseCanExecuteChanged();
        CopyPgnCommand.RaiseCanExecuteChanged();
    }

    private GameResult EvaluateGameResult()
    {
        if (_moveHistory.IsThreefoldRepetition()) return GameResult.ThreefoldRepetition;
        if (_moveHistory.IsFiftyMoves()) return GameResult.FiftyMoves;

        var moves = _position.GetAllMoves();
        if (moves.Count == 0)
            return _moveHistory.IsLastMoveWasCheck() ? GameResult.Mate : GameResult.Pat;

        return GameResult.Continue;
    }

    private void EndGame(GameResult result)
    {
        _clockTimer.Stop();
        IsGameOver = true;
        Board.BoardState = BoardState.ReadOnly;

        switch (result)
        {
            case GameResult.Mate:
                bool playerWins = _position.GetTurn() != _playerTurn;
                GameResultText = playerWins ? "You Win!" : "You Lose";
                GameResultDetail = "Checkmate";
                if (playerWins) _soundService.PlayWin(); else _soundService.PlayLose();
                break;
            case GameResult.Pat:
                GameResultText = "Draw";
                GameResultDetail = "Stalemate";
                _soundService.PlayDraw();
                break;
            case GameResult.ThreefoldRepetition:
                GameResultText = "Draw";
                GameResultDetail = "Threefold repetition";
                _soundService.PlayDraw();
                break;
            case GameResult.FiftyMoves:
                GameResultText = "Draw";
                GameResultDetail = "50-move rule";
                _soundService.PlayDraw();
                break;
        }

        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
        AnalyzeGameCommand.RaiseCanExecuteChanged();
    }

    #endregion

    #region Commands

    private bool CanUndo() => _movesPlayed.Count > 0;

    private void OnUndo()
    {
        if (!CanUndo()) return;

        _engineCts?.Cancel();
        IsEngineThinking = false;

        int undoCount = Math.Min(2, _movesPlayed.Count);
        for (int i = 0; i < undoCount; i++)
        {
            var move = _movesPlayed[^1];
            _undoneMovesStack.Push(move);
            _movesPlayed.RemoveAt(_movesPlayed.Count - 1);
            _position.UnMake();
        }

        Board.SyncFromPosition();
        RebuildMoveList();

        Board.BoardState = BoardState.Interactive;
        StatusText = "Move undone — your turn.";
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
    }

    private bool CanRedo() => _undoneMovesStack.Count > 0 && !IsGameOver;

    private void OnRedo()
    {
        if (!CanRedo()) return;

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
        RebuildMoveList();

        Board.BoardState = BoardState.Interactive;
        StatusText = "Move redone — your turn.";
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
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
                MoveList.Add(new MoveListItemModel { Number = moveNum, White = notation });
            }
            else if (MoveList.Count > 0)
            {
                MoveList[^1].Black = notation;
            }
        }
    }

    private bool CanResign() => !IsGameOver && _movesPlayed.Count > 0;

    private void OnResign()
    {
        _engineCts?.Cancel();
        _clockTimer.Stop();
        IsGameOver = true;
        Board.BoardState = BoardState.ReadOnly;
        GameResultText = "You Resigned";
        GameResultDetail = "Engine wins";
        _soundService.PlayLose();
        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
    }

    private bool CanOfferDraw() => !IsGameOver && _movesPlayed.Count > 0;

    private void OnOfferDraw()
    {
        _clockTimer.Stop();
        IsGameOver = true;
        Board.BoardState = BoardState.ReadOnly;
        GameResultText = "Draw";
        GameResultDetail = "1/2 - 1/2";
        _soundService.PlayDraw();
        ResignCommand.RaiseCanExecuteChanged();
        OfferDrawCommand.RaiseCanExecuteChanged();
    }

    private void OnDismissGameOver()
    {
        ShowGameOverOverlay = false;
    }

    private void OnClockTick(object sender, EventArgs e)
    {
        if (IsGameOver) return;

        var turn = _position.GetTurn();
        if (turn == Turn.White)
            _whiteTime = _whiteTime.Add(TimeSpan.FromSeconds(1));
        else
            _blackTime = _blackTime.Add(TimeSpan.FromSeconds(1));

        if (_playerTurn == Turn.White)
        {
            PlayerClock = _whiteTime.ToString(@"m\:ss");
            OpponentClock = _blackTime.ToString(@"m\:ss");
        }
        else
        {
            PlayerClock = _blackTime.ToString(@"m\:ss");
            OpponentClock = _whiteTime.ToString(@"m\:ss");
        }
    }

    // Analyze Game Command
    private bool CanAnalyzeGame() => _movesPlayed.Count > 0;

    private void OnAnalyzeGame()
    {
        if (_movesPlayed.Count == 0) return;

        try
        {
            // Build PGN for the game
            string pgn = BuildPgnString();

            // Get the path to the Analyzer executable
            var analyzerPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Analysis.Kio.Analyzer.exe");

            if (System.IO.File.Exists(analyzerPath))
            {
                // Write PGN to a temp file to avoid command line escaping issues
                var tempFile = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"kio_game_{DateTime.Now:yyyyMMddHHmmss}.pgn");
                System.IO.File.WriteAllText(tempFile, pgn);

                // Start the Analyzer application with PGN file path as argument
                var startInfo = new ProcessStartInfo
                {
                    FileName = analyzerPath,
                    Arguments = $"--pgn-file \"{tempFile}\"",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                StatusText = "Analyzer application started";
            }
            else
            {
                // Fallback: Copy PGN to clipboard
                Clipboard.SetText(pgn);
                StatusText = "PGN copied to clipboard. Open Analyzer and paste.";
                NotificationDialog.ShowInfo(
                    "Game PGN has been copied to clipboard.\n\nOpen the Analyzer application and use Load PGN to analyze the game.",
                    "Analyze Game");
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    private string BuildPgnString()
    {
        var sb = new System.Text.StringBuilder();

        // PGN headers
        sb.AppendLine("[Event \"Kio Chess Game\"]");
        sb.AppendLine("[Site \"Kio Chess Play\"]");
        sb.AppendLine($"[Date \"{DateTime.Now:yyyy.MM.dd}\"]");
        sb.AppendLine("[Round \"1\"]");
        sb.AppendLine($"[White \"{(_playerTurn == Turn.White ? "Player" : OpponentName)}\"]");
        sb.AppendLine($"[Black \"{(_playerTurn == Turn.Black ? "Player" : OpponentName)}\"]");

        string result = IsGameOver ? (GameResultText.Contains("Win") ?
            (_playerTurn == Turn.White ? "1-0" : "0-1") :
            (GameResultText.Contains("Lose") ?
                (_playerTurn == Turn.White ? "0-1" : "1-0") : "1/2-1/2")) : "*";
        sb.AppendLine($"[Result \"{result}\"]");
        sb.AppendLine();

        // Moves in UCI format (portable, unambiguous)
        for (int i = 0; i < _movesPlayed.Count; i++)
        {
            if (i % 2 == 0)
            {
                if (i > 0) sb.Append(' ');
                sb.Append($"{(i / 2) + 1}.");
            }
            sb.Append($" {UciMoveConverter.ToUci(_movesPlayed[i])}");
        }

        if (IsGameOver)
        {
            sb.Append($" {result}");
        }

        return sb.ToString();
    }

    #endregion

    #region Move Evaluation

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

            // Check if this is a book move
            bool isBookMove = await IsBookMoveAsync();

            if (isBookMove)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PlayerMoveClassification = MoveClassification.Book;
                    PlayerBestMoveSequence = string.Empty;
                    HasPlayerBestMoveSequence = false;
                });
                return;
            }

            // Evaluate with engine
            var evaluation = await _stockfish.EvaluateMoveAsync(movesBefore, playedMove, depth: 12);

            Application.Current.Dispatcher.Invoke(() =>
            {
                PlayerMoveClassification = evaluation.Classification;

                // Show best move sequence for non-optimal moves
                if (!string.IsNullOrEmpty(evaluation.BestMoveSequence) &&
                    evaluation.Classification != MoveClassification.Best &&
                    evaluation.Classification != MoveClassification.Brilliant &&
                    evaluation.Classification != MoveClassification.Book)
                {
                    var moves = evaluation.BestMoveSequence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    PlayerBestMoveSequence = string.Join(" ", moves.Take(Math.Min(4, moves.Length)));
                    HasPlayerBestMoveSequence = true;
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
        catch
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

            // Check if this is a book move
            bool isBookMove = await IsBookMoveAsync();

            if (isBookMove)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EngineMoveClassification = MoveClassification.Book;
                });
                return;
            }

            // Evaluate with engine
            var evaluation = await _stockfish.EvaluateMoveAsync(movesBefore, playedMove, depth: 12);

            Application.Current.Dispatcher.Invoke(() =>
            {
                EngineMoveClassification = evaluation.Classification;

                // Update board evaluation
                BoardEvaluation = evaluation.EvalAfter >= 0
                    ? $"+{evaluation.EvalAfter / 100.0:F2}"
                    : $"{evaluation.EvalAfter / 100.0:F2}";
            });
        }
        catch
        {
            // Silently fail - don't interrupt gameplay
        }
    }

    private async Task<bool> IsBookMoveAsync()
    {
        try
        {
            if (!_openingExplorer.IsInitialized()) return false;

            var positionKey = _movesPlayed.Select(m => m.Key).ToList();
            var opening = await _openingExplorer.GetOpeningsByMoveKeysAsync(positionKey);
            return opening?.Any() == true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Opening Detection

    private async Task DetectCurrentOpeningAsync()
    {
        try
        {
            if (!_openingExplorer.IsInitialized())
            {
                await _openingExplorer.ConnectAsync();
            }

            if (_movesPlayed.Count == 0)
            {
                HasOpeningData = false;
                IsOutOfBook = false;
                return;
            }

            var positionKey = _movesPlayed.Select(m => m.Key).ToList();
            var openings = await _openingExplorer.GetOpeningsByMoveKeysAsync(positionKey);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (openings != null && openings.Count > 0)
                {
                    var opening = openings[0];
                    CurrentOpeningName = opening.FullName;
                    CurrentOpeningECO = opening.ECO;
                    CurrentOpeningMoves = opening.MovesSAN;
                    HasOpeningData = true;
                    IsOutOfBook = false;
                }
                else if (HasOpeningData)
                {
                    // We had opening data before but now we're out of book
                    IsOutOfBook = true;
                }
            });
        }
        catch
        {
            // Silently fail
        }
    }

    #endregion

    #region Import/Export

    private void OnLoadPgn()
    {
        _dialogService.ShowOpenFile(
            "Load PGN File",
            "PGN Files (*.pgn)|*.pgn|All Files (*.*)|*.*",
            ".pgn",
            fileName =>
            {
                if (fileName != null)
                {
                    try
                    {
                        string pgnContent = System.IO.File.ReadAllText(fileName);
                        LoadPgnString(pgnContent);
                    }
                    catch (Exception ex)
                    {
                        NotificationDialog.ShowError($"Error loading PGN: {ex.Message}", "Load Error");
                    }
                }
            });
    }

    private void OnPastePgn()
    {
        string clipboardText = string.Empty;

        try
        {
            if (Clipboard.ContainsText())
            {
                clipboardText = Clipboard.GetText();
            }
        }
        catch
        {
            // Clipboard access can fail
        }

        _dialogService.ShowTextInput(
            "Paste PGN",
            "Paste or enter PGN notation:",
            clipboardText,
            "Load",
            pgnString =>
            {
                if (!string.IsNullOrEmpty(pgnString))
                {
                    LoadPgnString(pgnString);
                }
            },
            allowMultiline: true);
    }

    private void OnLoadFen()
    {
        _dialogService.ShowTextInput(
            "Load FEN Position",
            "Enter FEN string:",
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            "Load",
            fenString =>
            {
                if (!string.IsNullOrEmpty(fenString))
                {
                    try
                    {
                        LoadFenString(fenString);
                    }
                    catch (Exception ex)
                    {
                        NotificationDialog.ShowError($"Error loading FEN: {ex.Message}", "Load Error");
                    }
                }
            });
    }

    private void LoadPgnString(string pgn)
    {
        NotificationDialog.ShowInfo(
            "PGN loading in Play mode will start a new game from the starting position. " +
            "Use the Analyzer app to review and analyze PGN games.",
            "PGN Loading");
    }

    private void LoadFenString(string fen)
    {
        NotificationDialog.ShowInfo(
            "FEN loading in Play mode will start a new game from the starting position. " +
            "Custom FEN positions are not supported in Play mode. " +
            "Use the Analyzer app for FEN analysis.",
            "FEN Loading");
    }

    private bool CanSavePgn() => _movesPlayed.Count > 0;

    private void OnSavePgn()
    {
        if (_movesPlayed.Count == 0)
        {
            NotificationDialog.ShowWarning("No moves to save.", "Save PGN");
            return;
        }

        _dialogService.ShowSaveFile(
            "Save PGN File",
            "PGN Files (*.pgn)|*.pgn|All Files (*.*)|*.*",
            ".pgn",
            $"KioChess_{DateTime.Now:yyyy-MM-dd_HHmmss}.pgn",
            fileName =>
            {
                if (fileName != null)
                {
                    try
                    {
                        string pgnContent = BuildPgnString();
                        System.IO.File.WriteAllText(fileName, pgnContent);
                        NotificationDialog.ShowInfo($"Game saved to:\n{fileName}", "PGN Saved");
                    }
                    catch (Exception ex)
                    {
                        NotificationDialog.ShowError($"Error saving PGN: {ex.Message}", "Save Error");
                    }
                }
            });
    }

    private bool CanCopyPgn() => _movesPlayed.Count > 0;

    private void OnCopyPgn()
    {
        if (_movesPlayed.Count == 0)
        {
            NotificationDialog.ShowWarning("No moves to copy.", "Copy PGN");
            return;
        }

        try
        {
            string pgnContent = BuildPgnString();
            Clipboard.SetText(pgnContent);
            NotificationDialog.ShowInfo("PGN copied to clipboard!", "PGN Copied");
        }
        catch (Exception ex)
        {
            NotificationDialog.ShowError($"Error copying PGN: {ex.Message}", "Copy Error");
        }
    }

    private void OnCopyFen()
    {
        try
        {
            // Build FEN from current position
            string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // Placeholder
                                                                                     // TODO: Implement proper FEN export from Position

            Clipboard.SetText(fen);
            NotificationDialog.ShowInfo("FEN copied to clipboard!", "FEN Copied");
        }
        catch (Exception ex)
        {
            NotificationDialog.ShowError($"Error copying FEN: {ex.Message}", "Copy Error");
        }
    }

    private void OnOpenLibrary()
    {
        try
        {
            var libraryPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Analysis.Kio.Library.exe");

            if (File.Exists(libraryPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = libraryPath,
                    UseShellExecute = true,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                });
                StatusText = "Library application launched";
            }
            else
            {
                NotificationDialog.ShowWarning(
                    $"Library application not found at:\n{libraryPath}",
                    "Library Not Found");
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error launching Library: {ex.Message}";
            NotificationDialog.ShowError(
                $"Could not launch Library application:\n{ex.Message}",
                "Launch Error");
        }
    }

    private void OnOpenOpeningExplorer()
    {
        try
        {
            var explorerPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Analysis.Kio.OpeningsExplorer.exe");

            if (File.Exists(explorerPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = explorerPath,
                    UseShellExecute = true,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                });
                StatusText = "Opening Explorer launched";
            }
            else
            {
                NotificationDialog.ShowWarning(
                    $"Opening Explorer not found at:\n{explorerPath}",
                    "Opening Explorer Not Found");
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error launching Opening Explorer: {ex.Message}";
            NotificationDialog.ShowError(
                $"Could not launch Opening Explorer:\n{ex.Message}",
                "Launch Error");
        }
    }

    private void StartLocalEngine()
    {
        try
        {
            // Calculate relative path from Analysis folder to Application.exe
            // Analysis folder: C:\...\Analysis\
            // Application.exe: C:\...\Application\bin\Release\net9.0-windows7.0\Application.exe
            var enginePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "Application",
                "bin",
                "Release",
                "net9.0-windows7.0",
                "Application.exe");

            enginePath = Path.GetFullPath(enginePath);

            if (File.Exists(enginePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = enginePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(enginePath)
                });
                StatusText = "Local engine (Application.exe) launched";
            }
            else
            {
                NotificationDialog.ShowWarning(
                    $"Local engine not found at:\n{enginePath}\n\nPlease build the Application project first.",
                    "Engine Not Found");
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error launching local engine: {ex.Message}";
            NotificationDialog.ShowError(
                $"Could not launch local engine:\n{ex.Message}",
                "Launch Error");
        }
    }

    #endregion

    public void Dispose()
    {
        _clockTimer.Stop();
        _engineCts?.Cancel();
        _engineCts?.Dispose();
        Board.MoveMade -= OnHumanMoveMade;
    }
}
