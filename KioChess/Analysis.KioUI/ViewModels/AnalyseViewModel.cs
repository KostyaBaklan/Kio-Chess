using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.Core.Services;
using Analysis.DataAccess.Interfaces;
using Analysis.KioUI.Models;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Analysis.KioUI.ViewModels;

public class AnalyseViewModel : BindableBase
{
    private readonly IMoveFormatter _moveFormatter;
    private readonly IAnalysisService _analysisService;
    private readonly IOpeningExplorerService _openingExplorer;
    private readonly Services.IDialogService _dialogService;
    private readonly Position _position;
    private readonly MoveHistoryService _moveHistory;

    private readonly List<MoveBase> _gameMoves = [];
    private int _currentMoveIndex = -1;
    private CancellationTokenSource _analysisCts;

    public AnalyseViewModel(
        IMoveFormatter moveFormatter,
        IAnalysisService analysisService,
        IOpeningExplorerService openingExplorer,
        Services.IDialogService dialogService,
        Position position,
        MoveHistoryService moveHistory)
    {
        _moveFormatter = moveFormatter;
        _analysisService = analysisService;
        _openingExplorer = openingExplorer;
        _dialogService = dialogService;
        _position = position;
        _moveHistory = moveHistory;

        Board = new BoardViewModel { BoardState = BoardState.ReadOnly };
        MoveList = [];
        AnalysedMovesList = [];

        // Initialize board with starting position
        Board.LoadPosition(_position);
        Board.SyncFromPosition();

        StepForwardCommand = new DelegateCommand(OnStepForward, CanStepForward);
        StepBackCommand    = new DelegateCommand(OnStepBack,    CanStepBack);
        GoToStartCommand   = new DelegateCommand(OnGoToStart,   CanStepBack);
        GoToEndCommand     = new DelegateCommand(OnGoToEnd,     CanStepForward);
        AnalyseGameCommand = new DelegateCommand(OnAnalyseGame, () => IsNotAnalysing);
        LoadPgnCommand     = new DelegateCommand(OnLoadPgn);
        LoadFenCommand     = new DelegateCommand(OnLoadFen);
        ExportPgnCommand   = new DelegateCommand(OnExportPgn, () => _gameMoves.Count > 0);
        CancelAnalysisCommand = new DelegateCommand(OnCancelAnalysis, () => IsAnalysing);
        
        // Universal move filter navigation commands
        NavigateToMovesCommand = new DelegateCommand<string>(OnNavigateToMoves, CanNavigateToMoves);
        NextFilteredMoveCommand = new DelegateCommand(OnNextFilteredMove, CanNextFilteredMove);
        PreviousFilteredMoveCommand = new DelegateCommand(OnPreviousFilteredMove, CanPreviousFilteredMove);
        ClearMoveFilterCommand = new DelegateCommand(OnClearMoveFilter, () => IsMoveFilterActive);

        UpdateMoveIndexDisplay();
    }

    public BoardViewModel Board { get; }

    public DelegateCommand StepForwardCommand { get; }
    public DelegateCommand StepBackCommand    { get; }
    public DelegateCommand GoToStartCommand   { get; }
    public DelegateCommand GoToEndCommand     { get; }
    public DelegateCommand AnalyseGameCommand { get; }
    public DelegateCommand LoadPgnCommand     { get; }
    public DelegateCommand LoadFenCommand     { get; }
    public DelegateCommand ExportPgnCommand   { get; }
    public DelegateCommand CancelAnalysisCommand { get; }
    
    // Universal move filter navigation commands
    public DelegateCommand<string> NavigateToMovesCommand { get; }
    public DelegateCommand NextFilteredMoveCommand { get; }
    public DelegateCommand PreviousFilteredMoveCommand { get; }
    public DelegateCommand ClearMoveFilterCommand { get; }

    public ObservableCollection<MoveListItemViewModel> MoveList { get; }
    public ObservableCollection<AnalysedMoveViewModel> AnalysedMovesList { get; }

    private string _openingName = "—";
    public string OpeningName
    {
        get => _openingName;
        set => SetProperty(ref _openingName, value);
    }

    private string _openingECO = string.Empty;
    public string OpeningECO
    {
        get => _openingECO;
        set => SetProperty(ref _openingECO, value);
    }

    private string _openingMoves = string.Empty;
    public string OpeningMoves
    {
        get => _openingMoves;
        set => SetProperty(ref _openingMoves, value);
    }

    private bool _isInOpeningBook;
    public bool IsInOpeningBook
    {
        get => _isInOpeningBook;
        set => SetProperty(ref _isInOpeningBook, value);
    }

    private bool _hasOpeningData;
    public bool HasOpeningData
    {
        get => _hasOpeningData;
        set => SetProperty(ref _hasOpeningData, value);
    }

    private int _analysisDepth = 16;
    public int AnalysisDepth
    {
        get => _analysisDepth;
        set => SetProperty(ref _analysisDepth, value);
    }

    private bool _isAnalysing;
    public bool IsAnalysing
    {
        get => _isAnalysing;
        set
        {
            if (SetProperty(ref _isAnalysing, value))
            {
                RaisePropertyChanged(nameof(IsNotAnalysing));
                AnalyseGameCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public bool IsNotAnalysing => !_isAnalysing;

    private double _analysisProgress;
    public double AnalysisProgress
    {
        get => _analysisProgress;
        set => SetProperty(ref _analysisProgress, value);
    }

    private string _moveIndexDisplay = "Start";
    public string MoveIndexDisplay
    {
        get => _moveIndexDisplay;
        set => SetProperty(ref _moveIndexDisplay, value);
    }

    private const double BarTotal = 280.0;

    private double _evalBarWhiteHeight = BarTotal / 2;
    public double EvalBarWhiteHeight
    {
        get => _evalBarWhiteHeight;
        set => SetProperty(ref _evalBarWhiteHeight, value);
    }

    private double _evalBarBlackHeight = BarTotal / 2;
    public double EvalBarBlackHeight
    {
        get => _evalBarBlackHeight;
        set => SetProperty(ref _evalBarBlackHeight, value);
    }

    private string _evalText = "0.0";
    public string EvalText
    {
        get => _evalText;
        set => SetProperty(ref _evalText, value);
    }

    public void LoadGame(List<MoveBase> moves)
    {
        if (moves == null || moves.Count == 0)
        {
            StatusMessage = "No moves to load";
            return;
        }

        try
        {
            // Reset position to start
            while (_moveHistory.GetPly() >= 0)
            {
                _position.UnMake();
            }
            
            // Convert moves to UCI strings (portable format)
            var uciMoves = moves.Select(m => UciMoveConverter.ToUci(m)).ToList();
            
            // Recreate moves on this ViewModel's position
            _gameMoves.Clear();
            for (int i = 0; i < uciMoves.Count; i++)
            {
                var legalMoves = _position.GetAllMoves();
                var move = UciMoveConverter.FromUci(uciMoves[i], legalMoves);
                if (move != null)
                {
                    _gameMoves.Add(move);
                    
                    // Make the move to advance position for next iteration
                    if (i == 0)
                        _position.MakeFirst(move);
                    else
                        _position.Make(move);
                }
                else
                {
                    StatusMessage = $"Failed to parse move {i + 1}: {uciMoves[i]}";
                    break;
                }
            }
            
            // Now reset back to start so we can step through
            while (_moveHistory.GetPly() >= 0)
            {
                _position.UnMake();
            }
            
            _currentMoveIndex = -1;
            
            // Ensure board is loaded with this position
            Board.LoadPosition(_position);
            Board.SyncFromPosition();
            
            RebuildMoveList();
            UpdateCommandStates();
            UpdateMoveIndexDisplay();
            
            // Reset opening display for new game
            OpeningName = "—";
            OpeningECO = string.Empty;
            OpeningMoves = string.Empty;
            IsInOpeningBook = false;
            HasOpeningData = false;
            
            StatusMessage = $"Loaded {_gameMoves.Count} moves";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading game: {ex.Message}";
        }
    }

    private void OnStepForward()
    {
        if (!CanStepForward()) return;
        
        _currentMoveIndex++;
        
        // Make the move on the position
        if (_currentMoveIndex == 0)
            _position.MakeFirst(_gameMoves[_currentMoveIndex]);
        else
            _position.Make(_gameMoves[_currentMoveIndex]);
        
        Board.SyncFromPosition();
        HighlightCurrentMove();
        UpdateMoveIndexDisplay();
        UpdateCommandStates();
        UpdateCurrentMoveHighlight();
        
        // Detect opening for current position
        _ = DetectCurrentOpeningAsync();
    }

    private void OnStepBack()
    {
        if (!CanStepBack()) return;
        
        _position.UnMake();
        _currentMoveIndex--;
        
        Board.SyncFromPosition();
        HighlightCurrentMove();
        UpdateMoveIndexDisplay();
        UpdateCommandStates();
        UpdateCurrentMoveHighlight();
        
        // Detect opening for current position
        _ = DetectCurrentOpeningAsync();
    }

    private void OnGoToStart()
    {
        while (CanStepBack())
        {
            _position.UnMake();
            _currentMoveIndex--;
        }
        Board.SyncFromPosition();
        HighlightCurrentMove();
        UpdateMoveIndexDisplay();
        UpdateCommandStates();
        UpdateCurrentMoveHighlight();
        
        // Detect opening for current position
        _ = DetectCurrentOpeningAsync();
    }

    private void OnGoToEnd()
    {
        while (CanStepForward())
        {
            _currentMoveIndex++;
            if (_currentMoveIndex == 0)
                _position.MakeFirst(_gameMoves[_currentMoveIndex]);
            else
                _position.Make(_gameMoves[_currentMoveIndex]);
        }
        Board.SyncFromPosition();
        HighlightCurrentMove();
        UpdateMoveIndexDisplay();
        UpdateCommandStates();
        UpdateCurrentMoveHighlight();
        
        // Detect opening for current position
        _ = DetectCurrentOpeningAsync();
    }

    private bool CanStepForward() => _currentMoveIndex < _gameMoves.Count - 1;
    private bool CanStepBack()    => _currentMoveIndex >= 0;

    private void JumpToMove(int targetIndex)
    {
        if (targetIndex < -1 || targetIndex >= _gameMoves.Count)
            return;

        // Go back to start
        while (_currentMoveIndex >= 0)
        {
            _position.UnMake();
            _currentMoveIndex--;
        }

        // Step forward to target
        while (_currentMoveIndex < targetIndex)
        {
            _currentMoveIndex++;
            if (_currentMoveIndex == 0)
                _position.MakeFirst(_gameMoves[_currentMoveIndex]);
            else
                _position.Make(_gameMoves[_currentMoveIndex]);
        }

        Board.SyncFromPosition();
        HighlightCurrentMove();
        UpdateMoveIndexDisplay();
        UpdateCommandStates();
        UpdateCurrentMoveHighlight();
        
        // Detect opening for current position
        _ = DetectCurrentOpeningAsync();
    }

    private void HighlightCurrentMove()
    {
        foreach (var c in Board.Cells)
        {
            c.IsLastMoveFrom = false;
            c.IsLastMoveTo   = false;
        }
        if (_currentMoveIndex >= 0)
        {
            var move = _gameMoves[_currentMoveIndex];
            var from = Board.Cells.FirstOrDefault(c => c.Cell == move.From);
            var to   = Board.Cells.FirstOrDefault(c => c.Cell == move.To);
            if (from is not null) from.IsLastMoveFrom = true;
            if (to   is not null) to.IsLastMoveTo     = true;
        }
    }

    private void UpdateCommandStates()
    {
        StepForwardCommand.RaiseCanExecuteChanged();
        StepBackCommand.RaiseCanExecuteChanged();
        GoToStartCommand.RaiseCanExecuteChanged();
        GoToEndCommand.RaiseCanExecuteChanged();
    }

    private void UpdateMoveIndexDisplay()
    {
        if (_currentMoveIndex < 0) { MoveIndexDisplay = "Start"; return; }
        int full = _currentMoveIndex / 2 + 1;
        bool isBlack = _currentMoveIndex % 2 == 1;
        var notation = _moveFormatter.Format(_gameMoves[_currentMoveIndex]);
        MoveIndexDisplay = isBlack ? $"{full}... {notation}" : $"{full}. {notation}";
    }

    private void UpdateCurrentMoveHighlight()
    {
        // Update IsCurrentMove for all analyzed moves
        foreach (var move in AnalysedMovesList)
        {
            int moveIndex = (move.MoveNumber - 1) * 2 + (move.IsWhite ? 0 : 1);
            move.IsCurrentMove = (moveIndex == _currentMoveIndex);
        }
    }

    private void RebuildMoveList()
    {
        MoveList.Clear();
        for (int i = 0; i < _gameMoves.Count; i += 2)
        {
            MoveList.Add(new MoveListItemViewModel
            {
                Number = i / 2 + 1,
                White  = _moveFormatter.Format(_gameMoves[i]),
                Black  = i + 1 < _gameMoves.Count ? _moveFormatter.Format(_gameMoves[i + 1]) : string.Empty
            });
        }
    }

    private async void OnAnalyseGame()
    {
        if (_gameMoves.Count == 0)
        {
            StatusMessage = "No game loaded to analyze";
            return;
        }

        _analysisCts?.Cancel();
        _analysisCts = new CancellationTokenSource();
        
        IsAnalysing = true;
        AnalysisProgress = 0;
        StatusMessage = "Starting analysis...";
        
        try
        {
            var progress = new Progress<AnalysisProgress>(p =>
            {
                AnalysisProgress = p.PercentComplete;
                StatusMessage = p.Status;
            });

            var report = await _analysisService.AnalyseGameAsync(
                _gameMoves,
                _openingExplorer, // Pass opening explorer for book move detection
                AnalysisDepth, 
                progress, 
                _analysisCts.Token);

            Application.Current.Dispatcher.Invoke(() =>
            {
                DisplayAnalysisReport(report);
                StatusMessage = "Analysis complete!";
            });
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Analysis cancelled";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Analysis error: {ex.Message}";
        }
        finally
        {
            IsAnalysing = false;
            CancelAnalysisCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnCancelAnalysis()
    {
        _analysisCts?.Cancel();
    }

    private void OnLoadPgn()
    {
        var result = _dialogService.ShowTextInputDialog(
            "Load PGN",
            "Paste PGN text below. Include metadata tags like [Event], [White], etc., followed by the moves.",
            out string inputText);

        if (result == true && !string.IsNullOrWhiteSpace(inputText))
        {
            try
            {
                var pgnService = new PgnService(_position, _moveHistory);
                var parseResult = pgnService.ParsePgn(inputText);

                if (parseResult.IsSuccess && parseResult.Moves.Count > 0)
                {
                    LoadGame(parseResult.Moves);

                    // Update opening name if available
                    if (parseResult.Metadata.TryGetValue("Opening", out var opening))
                    {
                        OpeningName = opening;
                    }

                    StatusMessage = $"Loaded {parseResult.Moves.Count} moves from PGN";
                }
                else
                {
                    StatusMessage = parseResult.ErrorMessage ?? "Failed to parse PGN";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading PGN: {ex.Message}";
            }
        }
    }

    private void OnLoadFen()
    {
        var result = _dialogService.ShowTextInputDialog(
            "Load FEN",
            "Paste a FEN (Forsyth-Edwards Notation) string:\nExample: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            out string inputText);

        if (result == true && !string.IsNullOrWhiteSpace(inputText))
        {
            try
            {
                var fenString = inputText.Trim();

                if (FenService.ParseFen(_position, fenString))
                {
                    // Clear move list since FEN represents a specific position
                    _gameMoves.Clear();
                    _currentMoveIndex = -1;
                    MoveList.Clear();

                    // Load the position on the board
                    Board.LoadPosition(_position);
                    Board.SyncFromPosition();

                    UpdateCommandStates();
                    UpdateMoveIndexDisplay();

                    StatusMessage = "FEN position loaded successfully";
                }
                else
                {
                    StatusMessage = "Invalid FEN format";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading FEN: {ex.Message}";
            }
        }
    }

    private void OnExportPgn()
    {
        if (_gameMoves.Count == 0) return;
        
        var pgnService = new PgnService(_position, _moveHistory);
        var exportData = new PgnExportData
        {
            Event = "Analysis Game",
            Date = DateTime.Now,
            White = "White",
            Black = "Black",
            Moves = _gameMoves.Select((m, i) => new PgnMove
            {
                Notation = _moveFormatter.Format(m),
                Classification = MoveList.Count > i / 2 && i % 2 == 0 
                    ? MoveList[i / 2].WhiteClassification 
                    : MoveList.Count > i / 2 ? MoveList[i / 2].BlackClassification : null
            }).ToList()
        };
        
        var pgn = pgnService.ExportToPgn(exportData);
        Clipboard.SetText(pgn);
        StatusMessage = "PGN copied to clipboard";
    }

    private void DisplayAnalysisReport(GameAnalysisReport report)
    {
        AnalysedMovesList.Clear();
        
        foreach (var move in report.Moves)
        {
            AnalysedMovesList.Add(new AnalysedMoveViewModel(move, JumpToMove));
        }
        
        // Update statistics
        WhiteAccuracy = report.WhiteStatistics.Accuracy;
        BlackAccuracy = report.BlackStatistics.Accuracy;
        
        WhiteBestMoves = report.WhiteStatistics.BestMoves;
        WhiteExcellent = report.WhiteStatistics.ExcellentMoves;
        WhiteGood = report.WhiteStatistics.GoodMoves;
        WhiteBrilliant = report.WhiteStatistics.BrilliantMoves;
        WhiteBookMoves = report.WhiteStatistics.BookMoves;
        WhiteInaccuracies = report.WhiteStatistics.Inaccuracies;
        WhiteMistakes = report.WhiteStatistics.Mistakes;
        WhiteBlunders = report.WhiteStatistics.Blunders;
        
        BlackBestMoves = report.BlackStatistics.BestMoves;
        BlackExcellent = report.BlackStatistics.ExcellentMoves;
        BlackGood = report.BlackStatistics.GoodMoves;
        BlackBrilliant = report.BlackStatistics.BrilliantMoves;
        BlackBookMoves = report.BlackStatistics.BookMoves;
        BlackInaccuracies = report.BlackStatistics.Inaccuracies;
        BlackMistakes = report.BlackStatistics.Mistakes;
        BlackBlunders = report.BlackStatistics.Blunders;
        
        // Phase scores
        OpeningScore = report.PhaseScores.OpeningScore;
        MiddlegameScore = report.PhaseScores.MiddlegameScore;
        EndgameScore = report.PhaseScores.EndgameScore;
        
        OpeningGrade = report.PhaseScores.OpeningGrade;
        MiddlegameGrade = report.PhaseScores.MiddlegameGrade;
        EndgameGrade = report.PhaseScores.EndgameGrade;
        
        // Per-player phase accuracies
        WhiteOpeningAccuracy = report.PhaseScores.WhiteOpeningAccuracy;
        BlackOpeningAccuracy = report.PhaseScores.BlackOpeningAccuracy;
        WhiteMiddlegameAccuracy = report.PhaseScores.WhiteMiddlegameAccuracy;
        BlackMiddlegameAccuracy = report.PhaseScores.BlackMiddlegameAccuracy;
        WhiteEndgameAccuracy = report.PhaseScores.WhiteEndgameAccuracy;
        BlackEndgameAccuracy = report.PhaseScores.BlackEndgameAccuracy;
        
        // Update phase display properties
        RaisePropertyChanged(nameof(WhiteOpeningWidth));
        RaisePropertyChanged(nameof(WhiteOpeningDisplay));
        RaisePropertyChanged(nameof(BlackOpeningWidth));
        RaisePropertyChanged(nameof(BlackOpeningDisplay));
        RaisePropertyChanged(nameof(WhiteMiddlegameWidth));
        RaisePropertyChanged(nameof(WhiteMiddlegameDisplay));
        RaisePropertyChanged(nameof(BlackMiddlegameWidth));
        RaisePropertyChanged(nameof(BlackMiddlegameDisplay));
        RaisePropertyChanged(nameof(WhiteEndgameWidth));
        RaisePropertyChanged(nameof(WhiteEndgameDisplay));
        RaisePropertyChanged(nameof(BlackEndgameWidth));
        RaisePropertyChanged(nameof(BlackEndgameDisplay));
        
        // Update move list with classifications
        for (int i = 0; i < report.Moves.Count; i++)
        {
            var analysedMove = report.Moves[i];
            int listIndex = i / 2;
            
            if (listIndex < MoveList.Count)
            {
                if (analysedMove.IsWhite)
                    MoveList[listIndex].WhiteClassification = analysedMove.Classification.ToString();
                else
                    MoveList[listIndex].BlackClassification = analysedMove.Classification.ToString();
            }
        }
        
        HasAnalysisReport = true;
    }

    private string _statusMessage = "Load a game or paste PGN/FEN to begin";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    
    private bool _hasAnalysisReport;
    public bool HasAnalysisReport
    {
        get => _hasAnalysisReport;
        set
        {
            if (SetProperty(ref _hasAnalysisReport, value))
            {
                RaisePropertyChanged(nameof(HasNoAnalysisReport));
            }
        }
    }
    
    public bool HasNoAnalysisReport => !_hasAnalysisReport;
    
    // White statistics
    private double _whiteAccuracy;
    public double WhiteAccuracy
    {
        get => _whiteAccuracy;
        set => SetProperty(ref _whiteAccuracy, value);
    }
    
    private int _whiteBestMoves;
    public int WhiteBestMoves
    {
        get => _whiteBestMoves;
        set => SetProperty(ref _whiteBestMoves, value);
    }
    
    private int _whiteExcellent;
    public int WhiteExcellent
    {
        get => _whiteExcellent;
        set => SetProperty(ref _whiteExcellent, value);
    }
    
    private int _whiteGood;
    public int WhiteGood
    {
        get => _whiteGood;
        set => SetProperty(ref _whiteGood, value);
    }
    
    private int _whiteBrilliant;
    public int WhiteBrilliant
    {
        get => _whiteBrilliant;
        set => SetProperty(ref _whiteBrilliant, value);
    }
    
    private int _whiteBookMoves;
    public int WhiteBookMoves
    {
        get => _whiteBookMoves;
        set => SetProperty(ref _whiteBookMoves, value);
    }
    
    private int _whiteInaccuracies;
    public int WhiteInaccuracies
    {
        get => _whiteInaccuracies;
        set => SetProperty(ref _whiteInaccuracies, value);
    }
    
    private int _whiteMistakes;
    public int WhiteMistakes
    {
        get => _whiteMistakes;
        set => SetProperty(ref _whiteMistakes, value);
    }
    
    private int _whiteBlunders;
    public int WhiteBlunders
    {
        get => _whiteBlunders;
        set => SetProperty(ref _whiteBlunders, value);
    }
    
    // Black statistics
    private double _blackAccuracy;
    public double BlackAccuracy
    {
        get => _blackAccuracy;
        set => SetProperty(ref _blackAccuracy, value);
    }
    
    private int _blackBestMoves;
    public int BlackBestMoves
    {
        get => _blackBestMoves;
        set => SetProperty(ref _blackBestMoves, value);
    }
    
    private int _blackExcellent;
    public int BlackExcellent
    {
        get => _blackExcellent;
        set => SetProperty(ref _blackExcellent, value);
    }
    
    private int _blackGood;
    public int BlackGood
    {
        get => _blackGood;
        set => SetProperty(ref _blackGood, value);
    }
    
    private int _blackBrilliant;
    public int BlackBrilliant
    {
        get => _blackBrilliant;
        set => SetProperty(ref _blackBrilliant, value);
    }
    
    private int _blackBookMoves;
    public int BlackBookMoves
    {
        get => _blackBookMoves;
        set => SetProperty(ref _blackBookMoves, value);
    }
    
    private int _blackInaccuracies;
    public int BlackInaccuracies
    {
        get => _blackInaccuracies;
        set => SetProperty(ref _blackInaccuracies, value);
    }
    
    private int _blackMistakes;
    public int BlackMistakes
    {
        get => _blackMistakes;
        set => SetProperty(ref _blackMistakes, value);
    }
    
    private int _blackBlunders;
    public int BlackBlunders
    {
        get => _blackBlunders;
        set => SetProperty(ref _blackBlunders, value);
    }
    
    // Phase scores
    private double _openingScore;
    public double OpeningScore
    {
        get => _openingScore;
        set => SetProperty(ref _openingScore, value);
    }
    
    private double _middlegameScore;
    public double MiddlegameScore
    {
        get => _middlegameScore;
        set => SetProperty(ref _middlegameScore, value);
    }
    
    private double _endgameScore;
    public double EndgameScore
    {
        get => _endgameScore;
        set => SetProperty(ref _endgameScore, value);
    }
    
    private string _openingGrade = "−";
    public string OpeningGrade
    {
        get => _openingGrade;
        set => SetProperty(ref _openingGrade, value);
    }
    
    private string _middlegameGrade = "−";
    public string MiddlegameGrade
    {
        get => _middlegameGrade;
        set => SetProperty(ref _middlegameGrade, value);
    }
    
    private string _endgameGrade = "−";
    public string EndgameGrade
    {
        get => _endgameGrade;
        set => SetProperty(ref _endgameGrade, value);
    }

    // Per-player phase accuracies
    private double _whiteOpeningAccuracy;
    public double WhiteOpeningAccuracy
    {
        get => _whiteOpeningAccuracy;
        set => SetProperty(ref _whiteOpeningAccuracy, value);
    }
    
    private double _blackOpeningAccuracy;
    public double BlackOpeningAccuracy
    {
        get => _blackOpeningAccuracy;
        set => SetProperty(ref _blackOpeningAccuracy, value);
    }
    
    private double _whiteMiddlegameAccuracy;
    public double WhiteMiddlegameAccuracy
    {
        get => _whiteMiddlegameAccuracy;
        set => SetProperty(ref _whiteMiddlegameAccuracy, value);
    }
    
    private double _blackMiddlegameAccuracy;
    public double BlackMiddlegameAccuracy
    {
        get => _blackMiddlegameAccuracy;
        set => SetProperty(ref _blackMiddlegameAccuracy, value);
    }
    
    private double _whiteEndgameAccuracy;
    public double WhiteEndgameAccuracy
    {
        get => _whiteEndgameAccuracy;
        set => SetProperty(ref _whiteEndgameAccuracy, value);
    }
    
    private double _blackEndgameAccuracy;
    public double BlackEndgameAccuracy
    {
        get => _blackEndgameAccuracy;
        set => SetProperty(ref _blackEndgameAccuracy, value);
    }

    // Phase analysis display properties (White vs Black comparison)
    // For now, using same scores for both players - TODO: separate per player
    public double WhiteOpeningWidth => OpeningScore * 1.2; // Max width ~120px
    public string WhiteOpeningDisplay => $"{OpeningScore:F0}% ({OpeningGrade})";
    
    public double BlackOpeningWidth => OpeningScore * 1.2;
    public string BlackOpeningDisplay => $"{OpeningScore:F0}% ({OpeningGrade})";
    
    public double WhiteMiddlegameWidth => MiddlegameScore * 1.2;
    public string WhiteMiddlegameDisplay => $"{MiddlegameScore:F0}% ({MiddlegameGrade})";
    
    public double BlackMiddlegameWidth => MiddlegameScore * 1.2;
    public string BlackMiddlegameDisplay => $"{MiddlegameScore:F0}% ({MiddlegameGrade})";
    
    public double WhiteEndgameWidth => EndgameScore * 1.2;
    public string WhiteEndgameDisplay => $"{EndgameScore:F0}% ({EndgameGrade})";
    
    public double BlackEndgameWidth => EndgameScore * 1.2;
    public string BlackEndgameDisplay => $"{EndgameScore:F0}% ({EndgameGrade})";

    // Universal move filter navigation state
    private bool _isMoveFilterActive;
    public bool IsMoveFilterActive
    {
        get => _isMoveFilterActive;
        set
        {
            if (SetProperty(ref _isMoveFilterActive, value))
            {
                ClearMoveFilterCommand.RaiseCanExecuteChanged();
                NextFilteredMoveCommand.RaiseCanExecuteChanged();
                PreviousFilteredMoveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private MoveClassification? _filterClassification;
    private bool _filterIsWhite;
    private List<int> _filteredMoveIndices = new();
    private int _currentFilteredIndex = -1;

    private string _moveFilterText = string.Empty;
    public string MoveFilterText
    {
        get => _moveFilterText;
        set => SetProperty(ref _moveFilterText, value);
    }

    // – Opening Detection ––––––––––––––––––––––––––––––––––––––––––––
    private async Task DetectCurrentOpeningAsync()
    {
        try
        {
            if (_currentMoveIndex < 0)
            {
                OpeningName = "—";
                OpeningECO = string.Empty;
                OpeningMoves = string.Empty;
                IsInOpeningBook = false;
                HasOpeningData = false;
                return;
            }

            // Build position key from moves up to current index
            var movesUpToCurrent = _gameMoves.Take(_currentMoveIndex + 1);
            var moveKeys = movesUpToCurrent.Select(m => m.Key).ToList();

            // Look up opening in database
            var openings = await _openingExplorer.GetOpeningsByMoveKeysAsync(moveKeys);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (openings != null && openings.Count > 0)
                {
                    var opening = openings[0];
                    OpeningName = opening.FullName;
                    OpeningECO = opening.ECO;
                    OpeningMoves = opening.MovesSAN;
                    IsInOpeningBook = true;
                    HasOpeningData = true;
                }
                else
                {
                    // Out of book - keep last known opening
                    IsInOpeningBook = false;
                    // HasOpeningData and other properties remain unchanged
                }
            });
        }
        catch (Exception)
        {
            // Silently fail - don't interrupt analysis
        }
    }

    // Universal move filter navigation methods
    private bool CanNavigateToMoves(string parameter) => HasAnalysisReport && !string.IsNullOrEmpty(parameter);
    
    private void OnNavigateToMoves(string parameter)
    {
        if (string.IsNullOrEmpty(parameter)) return;
        
        // Parse parameter: "Classification,Color" (e.g., "Brilliant,White")
        var parts = parameter.Split(',');
        if (parts.Length != 2) return;
        
        if (!Enum.TryParse<MoveClassification>(parts[0], out var classification)) return;
        bool isWhite = parts[1].Equals("White", StringComparison.OrdinalIgnoreCase);
        
        StartMoveFilter(classification, isWhite);
    }

    private void StartMoveFilter(MoveClassification classification, bool isWhite)
    {
        _filteredMoveIndices.Clear();
        _filterClassification = classification;
        _filterIsWhite = isWhite;
        
        // Find all moves matching the filter
        for (int i = 0; i < AnalysedMovesList.Count; i++)
        {
            var move = AnalysedMovesList[i];
            if (move.IsWhite == isWhite && move.Classification == classification)
            {
                // Calculate move index
                int moveIndex = (move.MoveNumber - 1) * 2 + (move.IsWhite ? 0 : 1);
                _filteredMoveIndices.Add(moveIndex);
                
                // Mark move as filtered (for highlighting)
                move.IsFiltered = true;
            }
            else
            {
                move.IsFiltered = false;
            }
        }

        if (_filteredMoveIndices.Count > 0)
        {
            _currentFilteredIndex = 0;
            IsMoveFilterActive = true;
            JumpToMove(_filteredMoveIndices[0]);
            UpdateMoveFilterText();
        }
    }

    private bool CanNextFilteredMove() => IsMoveFilterActive && _filteredMoveIndices.Count > 0;
    
    private void OnNextFilteredMove()
    {
        if (!IsMoveFilterActive || _filteredMoveIndices.Count == 0) return;
        
        // Circular navigation: wrap around to beginning
        _currentFilteredIndex++;
        if (_currentFilteredIndex >= _filteredMoveIndices.Count)
            _currentFilteredIndex = 0;
        
        JumpToMove(_filteredMoveIndices[_currentFilteredIndex]);
        UpdateMoveFilterText();
    }

    private bool CanPreviousFilteredMove() => IsMoveFilterActive && _filteredMoveIndices.Count > 0;
    
    private void OnPreviousFilteredMove()
    {
        if (!IsMoveFilterActive || _filteredMoveIndices.Count == 0) return;
        
        // Circular navigation: wrap around to end
        _currentFilteredIndex--;
        if (_currentFilteredIndex < 0)
            _currentFilteredIndex = _filteredMoveIndices.Count - 1;
        
        JumpToMove(_filteredMoveIndices[_currentFilteredIndex]);
        UpdateMoveFilterText();
    }

    private void OnClearMoveFilter()
    {
        IsMoveFilterActive = false;
        _filteredMoveIndices.Clear();
        _currentFilteredIndex = -1;
        _filterClassification = null;
        MoveFilterText = string.Empty;
        
        // Clear filtering on all moves
        foreach (var move in AnalysedMovesList)
        {
            move.IsFiltered = false;
        }
    }

    private void UpdateMoveFilterText()
    {
        if (!IsMoveFilterActive || _filteredMoveIndices.Count == 0 || !_filterClassification.HasValue)
        {
            MoveFilterText = string.Empty;
            return;
        }

        var color = _filterIsWhite ? "White" : "Black";
        var classificationText = _filterClassification.Value switch
        {
            MoveClassification.Brilliant => "Brilliant",
            MoveClassification.Best => "Best",
            MoveClassification.Excellent => "Excellent",
            MoveClassification.Good => "Good",
            MoveClassification.Book => "Book",
            MoveClassification.Inaccuracy => "Inaccuracy",
            MoveClassification.Mistake => "Mistake",
            MoveClassification.Blunder => "Blunder",
            _ => "Moves"
        };
        
        MoveFilterText = $"{color} {classificationText} ({_currentFilteredIndex + 1} of {_filteredMoveIndices.Count})";
    }

    public void UpdateEvalBar(int centipawns)
    {
        double clamped = Math.Clamp(centipawns / 100.0, -10.0, 10.0);
        double whiteRatio = (clamped + 10.0) / 20.0;
        EvalBarWhiteHeight = whiteRatio * BarTotal;
        EvalBarBlackHeight = BarTotal - EvalBarWhiteHeight;
        EvalText = centipawns >= 0 ? $"+{centipawns / 100.0:F1}" : $"{centipawns / 100.0:F1}";
    }
}

