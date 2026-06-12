using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.DataAccess.Interfaces;
using Analysis.UI.Common.Models;
using Analysis.UI.Common.ViewModels;
using Analysis.UI.Common.Views;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Analysis.Kio.Analyzer.ViewModels;

public class AnalyseViewModel : BindableBase
{
    private readonly IMoveFormatter _moveFormatter;
    private readonly IAnalysisService _analysisService;
    private readonly IStockfishService _stockfish;
    private readonly ISettingsService _settingsService;
    private readonly IOpeningExplorerService _openingExplorer;
    private readonly IPgnParserService _pgnParser;
    private readonly Analysis.UI.Common.Services.IDialogService _dialogService;
    private readonly Position _position;
    private CancellationTokenSource _analysisCts;

    private readonly List<MoveBase> _gameMoves = [];
    private int _currentMoveIndex = -1;

    public AnalyseViewModel(
        IMoveFormatter moveFormatter,
        IAnalysisService analysisService,
        IStockfishService stockfish,
        ISettingsService settingsService,
        IOpeningExplorerService openingExplorer,
        IPgnParserService pgnParser,
        Analysis.UI.Common.Services.IDialogService dialogService,
        Position position)
    {
        _moveFormatter = moveFormatter;
        _analysisService = analysisService;
        _stockfish = stockfish;
        _settingsService = settingsService;
        _openingExplorer = openingExplorer;
        _pgnParser = pgnParser;
        _dialogService = dialogService;
        _position = position;

        Board = new BoardViewModel();
        Board.LoadPosition(_position);
        Board.BoardState = BoardState.ReadOnly;
        MoveList = [];

        StepForwardCommand = new DelegateCommand(OnStepForward, CanStepForward);
        StepBackCommand = new DelegateCommand(OnStepBack, CanStepBack);
        GoToStartCommand = new DelegateCommand(OnGoToStart, CanStepBack);
        GoToEndCommand = new DelegateCommand(OnGoToEnd, CanStepForward);
        AnalyseGameCommand = new DelegateCommand(OnAnalyseGame, CanAnalyseGame);
        LoadPgnCommand = new DelegateCommand(OnLoadPgn);
        LoadFenCommand = new DelegateCommand(OnLoadFen);
        PastePgnCommand = new DelegateCommand(OnPastePgn);
        NavigateToMovesCommand = new DelegateCommand<string>(OnNavigateToMoves, CanNavigateToMoves);
        NextFilteredMoveCommand = new DelegateCommand(OnNextFilteredMove, CanNextFilteredMove);
        PreviousFilteredMoveCommand = new DelegateCommand(OnPreviousFilteredMove, CanPreviousFilteredMove);
        ClearMoveFilterCommand = new DelegateCommand(OnClearMoveFilter, () => IsMoveFilterActive);

        UpdateMoveIndexDisplay();
        
        // Check for command line PGN argument
        ProcessCommandLineArgs();
    }

    public BoardViewModel Board { get; }
    public ObservableCollection<MoveListItemModel> MoveList { get; }
    public ObservableCollection<AnalysedMoveViewModel> AnalysedMovesList { get; } = [];

    public DelegateCommand StepForwardCommand { get; }
    public DelegateCommand StepBackCommand { get; }
    public DelegateCommand GoToStartCommand { get; }
    public DelegateCommand GoToEndCommand { get; }
    public DelegateCommand AnalyseGameCommand { get; }
    public DelegateCommand LoadPgnCommand { get; }
    public DelegateCommand LoadFenCommand { get; }
    public DelegateCommand PastePgnCommand { get; }
    public DelegateCommand<string> NavigateToMovesCommand { get; }
    public DelegateCommand NextFilteredMoveCommand { get; }
    public DelegateCommand PreviousFilteredMoveCommand { get; }
    public DelegateCommand ClearMoveFilterCommand { get; }

    #region Properties

    private string _moveIndexDisplay = "Start";
    public string MoveIndexDisplay
    {
        get => _moveIndexDisplay;
        set => SetProperty(ref _moveIndexDisplay, value);
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

    private string _statusMessage = "Load a game to begin";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _hasAnalysisReport;
    public bool HasAnalysisReport
    {
        get => _hasAnalysisReport;
        set => SetProperty(ref _hasAnalysisReport, value);
    }

    private double _whiteAccuracy;
    public double WhiteAccuracy
    {
        get => _whiteAccuracy;
        set
        {
            if (SetProperty(ref _whiteAccuracy, value))
            {
                RaisePropertyChanged(nameof(WhiteAccuracyText));
                RaisePropertyChanged(nameof(WhiteAccuracyBarWidth));
            }
        }
    }
    public string WhiteAccuracyText => $"{WhiteAccuracy:F1}%";
    public double WhiteAccuracyBarWidth => WhiteAccuracy * 2.0; // Max width ~200

    private double _blackAccuracy;
    public double BlackAccuracy
    {
        get => _blackAccuracy;
        set
        {
            if (SetProperty(ref _blackAccuracy, value))
            {
                RaisePropertyChanged(nameof(BlackAccuracyText));
                RaisePropertyChanged(nameof(BlackAccuracyBarWidth));
            }
        }
    }
    public string BlackAccuracyText => $"{BlackAccuracy:F1}%";
    public double BlackAccuracyBarWidth => BlackAccuracy * 2.0;

    // Phase Accuracy
    private double _whiteOpeningAccuracy;
    public double WhiteOpeningAccuracy 
    { 
        get => _whiteOpeningAccuracy; 
        set 
        { 
            if (SetProperty(ref _whiteOpeningAccuracy, value)) 
            {
                RaisePropertyChanged(nameof(WhiteOpeningWidth));
                RaisePropertyChanged(nameof(WhiteOpeningDisplay));
            }
        } 
    }
    public double WhiteOpeningWidth => WhiteOpeningAccuracy * 1.5;
    public string WhiteOpeningDisplay => $"{WhiteOpeningAccuracy:F1}%";

    private double _blackOpeningAccuracy;
    public double BlackOpeningAccuracy 
    { 
        get => _blackOpeningAccuracy; 
        set 
        { 
            if (SetProperty(ref _blackOpeningAccuracy, value)) 
            {
                RaisePropertyChanged(nameof(BlackOpeningWidth));
                RaisePropertyChanged(nameof(BlackOpeningDisplay));
            }
        } 
    }
    public double BlackOpeningWidth => BlackOpeningAccuracy * 1.5;
    public string BlackOpeningDisplay => $"{BlackOpeningAccuracy:F1}%";

    private double _whiteMiddlegameAccuracy;
    public double WhiteMiddlegameAccuracy 
    { 
        get => _whiteMiddlegameAccuracy; 
        set 
        { 
            if (SetProperty(ref _whiteMiddlegameAccuracy, value)) 
            {
                RaisePropertyChanged(nameof(WhiteMiddlegameWidth));
                RaisePropertyChanged(nameof(WhiteMiddlegameDisplay));
            }
        } 
    }
    public double WhiteMiddlegameWidth => WhiteMiddlegameAccuracy * 1.5;
    public string WhiteMiddlegameDisplay => $"{WhiteMiddlegameAccuracy:F1}%";

    private double _blackMiddlegameAccuracy;
    public double BlackMiddlegameAccuracy 
    { 
        get => _blackMiddlegameAccuracy; 
        set 
        { 
            if (SetProperty(ref _blackMiddlegameAccuracy, value)) 
            {
                RaisePropertyChanged(nameof(BlackMiddlegameWidth));
                RaisePropertyChanged(nameof(BlackMiddlegameDisplay));
            }
        } 
    }
    public double BlackMiddlegameWidth => BlackMiddlegameAccuracy * 1.5;
    public string BlackMiddlegameDisplay => $"{BlackMiddlegameAccuracy:F1}%";

    private double _whiteEndgameAccuracy;
    public double WhiteEndgameAccuracy 
    { 
        get => _whiteEndgameAccuracy; 
        set 
        { 
            if (SetProperty(ref _whiteEndgameAccuracy, value)) 
            {
                RaisePropertyChanged(nameof(WhiteEndgameWidth));
                RaisePropertyChanged(nameof(WhiteEndgameDisplay));
            }
        } 
    }
    public double WhiteEndgameWidth => WhiteEndgameAccuracy * 1.5;
    public string WhiteEndgameDisplay => $"{WhiteEndgameAccuracy:F1}%";

    private double _blackEndgameAccuracy;
    public double BlackEndgameAccuracy 
    { 
        get => _blackEndgameAccuracy; 
        set 
        { 
            if (SetProperty(ref _blackEndgameAccuracy, value)) 
            {
                RaisePropertyChanged(nameof(BlackEndgameWidth));
                RaisePropertyChanged(nameof(BlackEndgameDisplay));
            }
        } 
    }
    public double BlackEndgameWidth => BlackEndgameAccuracy * 1.5;
    public string BlackEndgameDisplay => $"{BlackEndgameAccuracy:F1}%";

    // Move Statistics
    private int _whiteBrilliant; public int WhiteBrilliant { get => _whiteBrilliant; set => SetProperty(ref _whiteBrilliant, value); }
    private int _blackBrilliant; public int BlackBrilliant { get => _blackBrilliant; set => SetProperty(ref _blackBrilliant, value); }
    private int _whiteBestMoves; public int WhiteBestMoves { get => _whiteBestMoves; set => SetProperty(ref _whiteBestMoves, value); }
    private int _blackBestMoves; public int BlackBestMoves { get => _blackBestMoves; set => SetProperty(ref _blackBestMoves, value); }
    private int _whiteExcellent; public int WhiteExcellent { get => _whiteExcellent; set => SetProperty(ref _whiteExcellent, value); }
    private int _blackExcellent; public int BlackExcellent { get => _blackExcellent; set => SetProperty(ref _blackExcellent, value); }
    private int _whiteGood; public int WhiteGood { get => _whiteGood; set => SetProperty(ref _whiteGood, value); }
    private int _blackGood; public int BlackGood { get => _blackGood; set => SetProperty(ref _blackGood, value); }
    private int _whiteBookMoves; public int WhiteBookMoves { get => _whiteBookMoves; set => SetProperty(ref _whiteBookMoves, value); }
    private int _blackBookMoves; public int BlackBookMoves { get => _blackBookMoves; set => SetProperty(ref _blackBookMoves, value); }
    private int _whiteInaccuracies; public int WhiteInaccuracies { get => _whiteInaccuracies; set => SetProperty(ref _whiteInaccuracies, value); }
    private int _blackInaccuracies; public int BlackInaccuracies { get => _blackInaccuracies; set => SetProperty(ref _blackInaccuracies, value); }
    private int _whiteMistakes; public int WhiteMistakes { get => _whiteMistakes; set => SetProperty(ref _whiteMistakes, value); }
    private int _blackMistakes; public int BlackMistakes { get => _blackMistakes; set => SetProperty(ref _blackMistakes, value); }
    private int _whiteBlunders; public int WhiteBlunders { get => _whiteBlunders; set => SetProperty(ref _whiteBlunders, value); }
    private int _blackBlunders; public int BlackBlunders { get => _blackBlunders; set => SetProperty(ref _blackBlunders, value); }

    // Move filter state
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

    #endregion

    #region Navigation

    private bool CanStepForward() => _currentMoveIndex < _gameMoves.Count - 1;
    private bool CanStepBack() => _currentMoveIndex >= 0;

    private void OnStepForward()
    {
        if (!CanStepForward()) return;
        _currentMoveIndex++;
        var move = _gameMoves[_currentMoveIndex];
        
        if (_currentMoveIndex == 0)
            _position.MakeFirst(move);
        else
            _position.Make(move);
            
        Board.SyncFromPosition();
        HighlightCurrentMove();
        UpdateMoveIndexDisplay();
        UpdateCurrentMoveHighlight();
        RaiseCommands();
    }

    private void OnStepBack()
    {
        if (!CanStepBack()) return;
        _position.UnMake();
        _currentMoveIndex--;
        Board.SyncFromPosition();
        HighlightCurrentMove();
        UpdateMoveIndexDisplay();
        UpdateCurrentMoveHighlight();
        RaiseCommands();
    }

    private void OnGoToStart()
    {
        while (CanStepBack())
        {
            _position.UnMake();
            _currentMoveIndex--;
        }
        Board.SyncFromPosition();
        ClearMoveHighlight();
        UpdateMoveIndexDisplay();
        UpdateCurrentMoveHighlight();
        RaiseCommands();
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
        UpdateCurrentMoveHighlight();
        RaiseCommands();
    }

    private void HighlightCurrentMove()
    {
        ClearMoveHighlight();
        if (_currentMoveIndex >= 0 && _currentMoveIndex < _gameMoves.Count)
        {
            var move = _gameMoves[_currentMoveIndex];
            Board.SetLastMoveHighlightPublic(move.From, move.To);
        }
    }

    private void ClearMoveHighlight()
    {
        foreach (var cell in Board.Cells)
        {
            cell.IsLastMoveFrom = false;
            cell.IsLastMoveTo = false;
        }
    }

    private void UpdateMoveIndexDisplay()
    {
        if (_currentMoveIndex < 0)
        {
            MoveIndexDisplay = "Start";
            return;
        }
        
        int moveNum = _currentMoveIndex / 2 + 1;
        bool isBlack = _currentMoveIndex % 2 == 1;
        var notation = _moveFormatter.Format(_gameMoves[_currentMoveIndex]);
        MoveIndexDisplay = isBlack ? $"{moveNum}... {notation}" : $"{moveNum}. {notation}";
    }

    private void RaiseCommands()
    {
        StepForwardCommand.RaiseCanExecuteChanged();
        StepBackCommand.RaiseCanExecuteChanged();
        GoToStartCommand.RaiseCanExecuteChanged();
        GoToEndCommand.RaiseCanExecuteChanged();
        AnalyseGameCommand.RaiseCanExecuteChanged();
    }

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
        UpdateCurrentMoveHighlight();
        RaiseCommands();
    }




    private void UpdateCurrentMoveHighlight()
    {
        foreach (var move in AnalysedMovesList)
        {
            int moveIndex = (move.MoveNumber - 1) * 2 + (move.IsWhite ? 0 : 1);
            move.IsCurrentMove = (moveIndex == _currentMoveIndex);
        }
    }

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

    #endregion

    #region Analysis

    private bool CanAnalyseGame() => _gameMoves.Count > 0 && IsNotAnalysing;

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
        StatusMessage = "Initializing engine...";
        HasAnalysisReport = false;

        try
        {
            // Initialize Stockfish if not ready
            if (!_stockfish.IsReady)
            {
                var path = _settingsService.Current.StockfishPath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    StatusMessage = "Stockfish not found. Update path in Settings.";
                    IsAnalysing = false;
                    return;
                }

                await _stockfish.InitialiseAsync(path);
                await _stockfish.ConfigureAsync(new StockfishOptions
                {
                    HashSizeMB = 128,
                    Threads = 1,
                    MultiPV = 2,
                    UsePositionHistory = false
                });
            }

            StatusMessage = "Starting analysis...";

            var progress = new Progress<AnalysisProgress>(p =>
            {
                AnalysisProgress = p.PercentComplete;
                StatusMessage = p.Status;
            });

            var report = await _analysisService.AnalyseGameAsync(
                _gameMoves,
                _openingExplorer,
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
        }
    }

    private void DisplayAnalysisReport(GameAnalysisReport report)
    {
        // Overall accuracy
        WhiteAccuracy = report.WhiteStatistics.Accuracy;
        BlackAccuracy = report.BlackStatistics.Accuracy;
        
        // Move statistics
        WhiteBrilliant = report.WhiteStatistics.BrilliantMoves;
        BlackBrilliant = report.BlackStatistics.BrilliantMoves;
        WhiteBestMoves = report.WhiteStatistics.BestMoves;
        BlackBestMoves = report.BlackStatistics.BestMoves;
        WhiteExcellent = report.WhiteStatistics.ExcellentMoves;
        BlackExcellent = report.BlackStatistics.ExcellentMoves;
        WhiteGood = report.WhiteStatistics.GoodMoves;
        BlackGood = report.BlackStatistics.GoodMoves;
        WhiteBookMoves = report.WhiteStatistics.BookMoves;
        BlackBookMoves = report.BlackStatistics.BookMoves;
        WhiteInaccuracies = report.WhiteStatistics.Inaccuracies;
        BlackInaccuracies = report.BlackStatistics.Inaccuracies;
        WhiteMistakes = report.WhiteStatistics.Mistakes;
        BlackMistakes = report.BlackStatistics.Mistakes;
        WhiteBlunders = report.WhiteStatistics.Blunders;
        BlackBlunders = report.BlackStatistics.Blunders;
        
        // Phase accuracy
        WhiteOpeningAccuracy = report.PhaseScores.WhiteOpeningAccuracy;
        BlackOpeningAccuracy = report.PhaseScores.BlackOpeningAccuracy;
        WhiteMiddlegameAccuracy = report.PhaseScores.WhiteMiddlegameAccuracy;
        BlackMiddlegameAccuracy = report.PhaseScores.BlackMiddlegameAccuracy;
        WhiteEndgameAccuracy = report.PhaseScores.WhiteEndgameAccuracy;
        BlackEndgameAccuracy = report.PhaseScores.BlackEndgameAccuracy;
        
        // Populate analyzed moves list for move report
        AnalysedMovesList.Clear();
        foreach (var move in report.Moves)
        {
            AnalysedMovesList.Add(new AnalysedMoveViewModel(move, JumpToMove));
        }
        
        HasAnalysisReport = true;

        // Keep simple move list for basic display
        MoveList.Clear();
        for (int i = 0; i < _gameMoves.Count; i += 2)
        {
            var item = new MoveListItemModel
            {
                Number = i / 2 + 1,
                White = _moveFormatter.Format(_gameMoves[i]),
                Black = i + 1 < _gameMoves.Count ? _moveFormatter.Format(_gameMoves[i + 1]) : string.Empty
            };
            MoveList.Add(item);
        }
    }

    #endregion

    #region Load/Save

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
                        string pgnContent = File.ReadAllText(fileName);
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
                    try
                    {
                        LoadPgnString(pgnString);
                    }
                    catch (Exception ex)
                    {
                        NotificationDialog.ShowError($"Error loading PGN: {ex.Message}", "Load Error");
                    }
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
                        NotificationDialog.ShowError($"Error loading PGN: {ex.Message}", "Load Error");
                    }
                }
            });
    }


    private void ProcessCommandLineArgs()
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--pgn-file" && i + 1 < args.Length)
            {
                var pgnFilePath = args[i + 1];
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (File.Exists(pgnFilePath))
                        {
                            var pgnContent = File.ReadAllText(pgnFilePath);
                            LoadPgnString(pgnContent);
                            try { File.Delete(pgnFilePath); } catch { }
                        }
                }
                catch (Exception ex)
                {
                    NotificationDialog.ShowError($"Error loading PGN file: {ex.Message}", "Load Error");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            break;
        }
        
        if (args[i] == "--pgn" && i + 1 < args.Length)
        {
            var pgnContent = args[i + 1];
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadPgnString(pgnContent);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            break;
        }
    }
}

private void LoadPgnString(string pgn)
{
    try
    {
        var moves = _pgnParser.ParseMoves(pgn);
        if (moves != null && moves.Count > 0)
        {
            LoadGame(moves);
            StatusMessage = $"Loaded {moves.Count} moves";
        }
        else
        {
            StatusMessage = "No moves found in PGN";
        }
    }
    catch (Exception ex)
    {
        NotificationDialog.ShowWarning($"Error parsing PGN: {ex.Message}", "Parse Error");
    }
}

    private void LoadFenString(string fen)
    {
        _gameMoves.Clear();
        MoveList.Clear();
        _currentMoveIndex = -1;
        HasAnalysisReport = false;
    
        _position.Clear();
        Board.LoadPosition(_position);
        Board.SyncFromPosition();
    
        NotificationDialog.ShowInfo(
            "FEN loading will set the starting position. Custom FEN positions are not yet supported.", 
            "FEN Loading");
    
        UpdateMoveIndexDisplay();
        RaiseCommands();
    }

    public void LoadGame(List<MoveBase> moves)
    {
        _gameMoves.Clear();
        _gameMoves.AddRange(moves);
        _currentMoveIndex = -1;
        HasAnalysisReport = false;

        _position.Clear();
        Board.LoadPosition(_position);
        Board.SyncFromPosition();

        MoveList.Clear();
        for (int i = 0; i < moves.Count; i += 2)
        {
            MoveList.Add(new MoveListItemModel
            {
                Number = i / 2 + 1,
                White = _moveFormatter.Format(moves[i]),
                Black = i + 1 < moves.Count ? _moveFormatter.Format(moves[i + 1]) : string.Empty
            });
        }

        UpdateMoveIndexDisplay();
        RaiseCommands();
    }

    #endregion
}

public class MoveListItemModel : BindableBase
{
    private int _number;
    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    private string _white = string.Empty;
    public string White
    {
        get => _white;
        set => SetProperty(ref _white, value);
    }

    private string _black = string.Empty;
    public string Black
    {
        get => _black;
        set => SetProperty(ref _black, value);
    }
}
