using Engine.DataStructures;
using Engine.Dal.Interfaces;
using Microsoft.Win32;
using StockfishBenchmark.Models;
using StockfishBenchmark.Services;
using System.Windows.Input;

namespace StockfishBenchmark.ViewModels;

public class TtSetupViewModel : BindableBase
{
    private readonly Action<BindableBase> _navigate;
    private BenchmarkResult _loadedBaseline;

    public string[] Strategies { get; } = { "lmrd", "id", "asp" };
    public int[] Depths { get; } = { 7, 8, 9, 10, 11, 12 };

    private string _selectedStrategy = "lmrd";
    public string SelectedStrategy { get => _selectedStrategy; set => SetProperty(ref _selectedStrategy, value); }

    private int _selectedDepth = 10;
    public int SelectedDepth { get => _selectedDepth; set => SetProperty(ref _selectedDepth, value); }

    public string[] ColorLabels { get; } =
    {
        "Engine plays White  (Stockfish=Black)",
        "Engine plays Black  (Stockfish=White)"
    };

    private string _selectedColorLabel;
    public string SelectedColorLabel { get => _selectedColorLabel; set => SetProperty(ref _selectedColorLabel, value); }
    public string ColorCode => SelectedColorLabel == ColorLabels[0] ? "b" : "w";

    private int _moveCount = 24;
    public int MoveCount { get => _moveCount; set => SetProperty(ref _moveCount, value); }

    private int _stockfishDepth = 11;
    public int StockfishDepth { get => _stockfishDepth; set => SetProperty(ref _stockfishDepth, value); }

    private int _stockfishElo = 2600;
    public int StockfishElo { get => _stockfishElo; set => SetProperty(ref _stockfishElo, value); }

    private bool _isNewGame = true;
    public bool IsNewGame
    {
        get => _isNewGame;
        set { if (SetProperty(ref _isNewGame, value)) RaisePropertyChanged(nameof(IsLoadMode)); }
    }

    public bool IsLoadMode { get => !_isNewGame; set => IsNewGame = !value; }

    private string _loadedFilePath = string.Empty;
    public string LoadedFilePath { get => _loadedFilePath; private set => SetProperty(ref _loadedFilePath, value); }

    private string _mainSizes = "0";
    public string MainSizes { get => _mainSizes; set => SetProperty(ref _mainSizes, value); }

    private string _nullSizes = "0";
    public string NullSizes { get => _nullSizes; set => SetProperty(ref _nullSizes, value); }

    private string _depthFactors = "-1";
    public string DepthFactors { get => _depthFactors; set => SetProperty(ref _depthFactors, value); }

    public string StatsBuildText => TtStatistics.Enabled
        ? "Engine built with TT_STATS: full statistics will be collected."
        : "Engine built WITHOUT TT_STATS: only timing is measured. Set <TtStats>true</TtStats> in Engine.csproj and rebuild.";

    private string _validationMessage = string.Empty;
    public string ValidationMessage { get => _validationMessage; set => SetProperty(ref _validationMessage, value); }

    public ICommand StartCommand { get; }
    public ICommand LoadFileCommand { get; }
    public ICommand BackCommand { get; }

    public TtSetupViewModel(Action<BindableBase> navigate)
    {
        _navigate = navigate;
        _selectedColorLabel = ColorLabels[1];

        StartCommand    = new DelegateCommand(ExecuteStart);
        LoadFileCommand = new DelegateCommand(ExecuteLoadFile);
        BackCommand     = new DelegateCommand(() => _navigate(new StartViewModel(_navigate)));
    }

    private void ExecuteLoadFile()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Open game recorded by a benchmark run",
            Filter = "Benchmark JSON|*.json|All files|*.*"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            _loadedBaseline = ContainerLocator.Current.Resolve<IBenchmarkFileService>().Load(dlg.FileName);
            LoadedFilePath = dlg.FileName;
            ValidationMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Failed to load file: {ex.Message}";
        }
    }

    private void ExecuteStart()
    {
        if (IsLoadMode && _loadedBaseline == null)
        {
            ValidationMessage = "Please browse to a recorded game first.";
            return;
        }
        if (!TryParseSizes(MainSizes, out var mains) || !TryParseSizes(NullSizes, out var nulls))
        {
            ValidationMessage = "Table sizes must be comma separated integers in MB (0 = engine default).";
            return;
        }

        var factors = new List<int>();
        foreach (var part in (DepthFactors ?? string.Empty).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(part.Trim(), out var f) || f < -1 || f > 12)
            {
                ValidationMessage = "Depth factors must be comma separated integers 0..12 (-1 = value from Configuration.json).";
                return;
            }
            factors.Add(f);
        }
        if (factors.Count == 0) factors.Add(-1);
        factors = factors.Distinct().ToList();

        var variants = new List<TtVariant>();
        foreach (var m in mains)
            foreach (var n in nulls)
                foreach (var f in factors)
                    variants.Add(new TtVariant
                    {
                        Name = $"main={(m == 0 ? "def" : m + "MB")} null={(n == 0 ? "def" : n + "MB")} df={(f < 0 ? "cfg" : f)}",
                        MainMB = m,
                        NullMB = n,
                        DepthFactor = f,
                        AdvanceMain = true,
                        AdvanceNull = true
                    });

        ValidationMessage = string.Empty;
        ContainerLocator.Current.Resolve<ICacheLoaderService>().WaitToData();

        var request = new TtBenchmarkRequest
        {
            Baseline = IsLoadMode ? _loadedBaseline : null,
            Variants = variants,
            Session = IsLoadMode ? _loadedBaseline.Session : new BenchmarkSession
            {
                Strategy       = SelectedStrategy,
                Depth          = (short)SelectedDepth,
                Color          = ColorCode,
                MoveCount      = int.MaxValue,
                StockfishDepth = (short)StockfishDepth,
                StockfishElo   = StockfishElo,
                RunDate        = DateTime.Now,
                MachineName    = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                RuntimeVersion = Environment.Version.ToString()
            }
        };
        _navigate(new TtRunViewModel(request, _navigate));
    }

    private static bool TryParseSizes(string text, out List<int> sizes)
    {
        sizes = new List<int>();
        foreach (var part in (text ?? string.Empty).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var p = part.Trim();
            if (p.Equals("default", StringComparison.OrdinalIgnoreCase) || p.Equals("def", StringComparison.OrdinalIgnoreCase))
            {
                sizes.Add(0);
            }
            else if (int.TryParse(p, out var v) && v >= 0)
            {
                sizes.Add(v);
            }
            else
            {
                return false;
            }
        }
        if (sizes.Count == 0) sizes.Add(0);
        sizes = sizes.Distinct().ToList();
        return true;
    }
}
