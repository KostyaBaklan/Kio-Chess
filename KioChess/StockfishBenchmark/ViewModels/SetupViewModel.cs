using Microsoft.Win32;
using StockfishBenchmark.Models;
using StockfishBenchmark.Services;
using System.Windows.Input;

namespace StockfishBenchmark.ViewModels;

public class SetupViewModel : BindableBase
{
    private readonly Action<BindableBase> _navigate;

    // ── Strategy ──────────────────────────────────────────────────────────
    public string[] Strategies { get; } = { "lmrd", "id", "asp", "ab" };

    private string _selectedStrategy = "lmrd";
    public string SelectedStrategy
    {
        get => _selectedStrategy;
        set => SetProperty(ref _selectedStrategy, value);
    }

    // ── Depth ─────────────────────────────────────────────────────────────
    public int[] Depths { get; } = { 8, 9, 10, 11 };

    private int _selectedDepth = 10;
    public int SelectedDepth
    {
        get => _selectedDepth;
        set => SetProperty(ref _selectedDepth, value);
    }

    // ── Engine colour ─────────────────────────────────────────────────────
    // Color code: "w" = Stockfish plays White (engine plays Black),
    //             "b" = Stockfish plays Black (engine plays White)
    public string[] ColorLabels { get; } =
    {
        "Engine plays White  (Stockfish=Black)",
        "Engine plays Black  (Stockfish=White)"
    };

    private string _selectedColorLabel;
    public string SelectedColorLabel
    {
        get => _selectedColorLabel;
        set
        {
            SetProperty(ref _selectedColorLabel, value);
            RaisePropertyChanged(nameof(ColorCode));
        }
    }

    // "b" when engine is White (Stockfish Black), "w" when engine is Black
    public string ColorCode => SelectedColorLabel == ColorLabels[0] ? "b" : "w";

    // ── Numeric parameters ────────────────────────────────────────────────
    private int _moveCount = 24;
    public int MoveCount
    {
        get => _moveCount;
        set => SetProperty(ref _moveCount, value);
    }

    private int _stockfishDepth = 5;
    public int StockfishDepth
    {
        get => _stockfishDepth;
        set => SetProperty(ref _stockfishDepth, value);
    }

    private int _stockfishElo = 1500;
    public int StockfishElo
    {
        get => _stockfishElo;
        set => SetProperty(ref _stockfishElo, value);
    }

    // ── Mode ──────────────────────────────────────────────────────────────
    private bool _isNewRun = true;
    public bool IsNewRun
    {
        get => _isNewRun;
        set
        {
            if (SetProperty(ref _isNewRun, value))
                RaisePropertyChanged(nameof(IsLoadMode));
        }
    }

    public bool IsLoadMode
    {
        get => !_isNewRun;
        set => IsNewRun = !value;
    }

    private string _loadedFilePath = string.Empty;
    public string LoadedFilePath
    {
        get => _loadedFilePath;
        private set => SetProperty(ref _loadedFilePath, value);
    }

    private BenchmarkResult _loadedBaseline;

    // ── Validation ────────────────────────────────────────────────────────
    private string _validationMessage = string.Empty;
    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    // ── Commands ──────────────────────────────────────────────────────────
    public ICommand StartCommand     { get; }
    public ICommand LoadFileCommand  { get; }

    public SetupViewModel(Action<BindableBase> navigate)
    {
        _navigate = navigate;
        _selectedColorLabel = ColorLabels[0];

        StartCommand    = new DelegateCommand(ExecuteStart);
        LoadFileCommand = new DelegateCommand(ExecuteLoadFile);
    }

    private void ExecuteStart()
    {
        if (!Validate()) return;

        if (IsLoadMode && _loadedBaseline != null)
        {
            var runVm = new RunViewModel(_loadedBaseline, _navigate);
            _navigate(runVm);
        }
        else
        {
            var session = BuildSession();
            var runVm   = new RunViewModel(session, _navigate);
            _navigate(runVm);
        }
    }

    private void ExecuteLoadFile()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Open Benchmark Baseline",
            Filter = "Benchmark JSON|*.json|All files|*.*"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            var svc             = ContainerLocator.Current.Resolve<IBenchmarkFileService>();
            _loadedBaseline     = svc.Load(dlg.FileName);
            LoadedFilePath      = dlg.FileName;
            IsLoadMode          = true;
            ValidationMessage   = string.Empty;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Failed to load file: {ex.Message}";
        }
    }

    private bool Validate()
    {
        if (IsLoadMode && _loadedBaseline == null)
        {
            ValidationMessage = "Please browse to a baseline file first.";
            return false;
        }
        if (MoveCount is < 1 or > 100)
        {
            ValidationMessage = "Move count must be between 1 and 100.";
            return false;
        }
        if (StockfishDepth is < 2 or > 15)
        {
            ValidationMessage = "Stockfish depth must be between 2 and 15.";
            return false;
        }
        if (StockfishElo is < 1320 or > 3190)
        {
            ValidationMessage = "Stockfish Elo must be between 1320 and 3190.";
            return false;
        }

        ValidationMessage = string.Empty;
        return true;
    }

    private BenchmarkSession BuildSession() => new()
    {
        Strategy       = SelectedStrategy,
        Depth          = (short)SelectedDepth,
        Color          = ColorCode,
        MoveCount      = MoveCount,
        StockfishDepth = (short)StockfishDepth,
        StockfishElo   = StockfishElo,
        RunDate        = DateTime.Now,
        MachineName    = Environment.MachineName,
        ProcessorCount = Environment.ProcessorCount,
        RuntimeVersion = Environment.Version.ToString()
    };
}
