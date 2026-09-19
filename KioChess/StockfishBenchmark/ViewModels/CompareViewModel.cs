using Microsoft.Win32;
using StockfishBenchmark.Converters;
using StockfishBenchmark.Models;
using StockfishBenchmark.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace StockfishBenchmark.ViewModels;

public class CompareViewModel : BindableBase
{
    private readonly Action<BindableBase> _navigate;

    // ── Session data ──────────────────────────────────────────────────────
    private BenchmarkResult _baseline;
    public BenchmarkResult Baseline
    {
        get => _baseline;
        private set { SetProperty(ref _baseline, value); RaisePropertyChanged(nameof(BaselineInfo)); }
    }

    private BenchmarkResult _newRun;
    public BenchmarkResult NewRun
    {
        get => _newRun;
        private set { SetProperty(ref _newRun, value); RaisePropertyChanged(nameof(NewRunInfo)); }
    }

    public string BaselineInfo => Baseline?.Session?.DisplayName ?? "(none)";
    public string NewRunInfo   => NewRun?.Session?.DisplayName   ?? "(none)";

    // ── Comparison rows ───────────────────────────────────────────────────
    public ObservableCollection<ComparisonRow> Rows { get; } = new();

    // ── Summary stats ─────────────────────────────────────────────────────
    private string _avgDeltaText = "—";
    public string AvgDeltaText
    {
        get => _avgDeltaText;
        set => SetProperty(ref _avgDeltaText, value);
    }

    private string _totalTimeDeltaText = "—";
    public string TotalTimeDeltaText
    {
        get => _totalTimeDeltaText;
        set => SetProperty(ref _totalTimeDeltaText, value);
    }

    private int _regressionCount;
    public int RegressionCount
    {
        get => _regressionCount;
        set { SetProperty(ref _regressionCount, value); RaisePropertyChanged(nameof(HasRegressions)); }
    }
    public bool HasRegressions => RegressionCount > 0;

    private double _baselineAvg;
    public double BaselineAvg { get => _baselineAvg; set => SetProperty(ref _baselineAvg, value); }

    private double _newAvg;
    public double NewAvg { get => _newAvg; set => SetProperty(ref _newAvg, value); }

    private double _baselineMax;
    public double BaselineMax { get => _baselineMax; set => SetProperty(ref _baselineMax, value); }

    private double _newMax;
    public double NewMax { get => _newMax; set => SetProperty(ref _newMax, value); }

    private double _baselineTtAvg;
    public double BaselineTtAvg { get => _baselineTtAvg; set => SetProperty(ref _baselineTtAvg, value); }

    private double _newTtAvg;
    public double NewTtAvg { get => _newTtAvg; set => SetProperty(ref _newTtAvg, value); }

    // ── Commands ──────────────────────────────────────────────────────────
    public ICommand LoadBaselineCommand { get; }
    public ICommand LoadNewRunCommand   { get; }
    public ICommand BackCommand         { get; }
    public ICommand ExportCsvCommand    { get; }

    public CompareViewModel(BenchmarkResult baseline, BenchmarkResult newRun,
                            Action<BindableBase> navigate)
    {
        _navigate = navigate;
        Baseline  = baseline;
        NewRun    = newRun;

        LoadBaselineCommand = new DelegateCommand(ExecuteLoadBaseline);
        LoadNewRunCommand   = new DelegateCommand(ExecuteLoadNewRun);
        BackCommand         = new DelegateCommand(ExecuteBack);
        ExportCsvCommand    = new DelegateCommand(ExecuteExportCsv);

        BuildRows();
    }

    // ── Row construction ─────────────────────────────────────────────────
    private void BuildRows()
    {
        Rows.Clear();
        if (Baseline == null || NewRun == null) return;

        var baseEngMoves = Baseline.Moves.Where(m => m.IsEngineMove).ToList();
        var newEngMoves  = NewRun.Moves.Where(m => m.IsEngineMove).ToList();
        int count = Math.Min(baseEngMoves.Count, newEngMoves.Count);

        bool diverged = false;
        for (int i = 0; i < count; i++)
        {
            var b = baseEngMoves[i];
            var n = newEngMoves[i];

            bool isRegression = n.UciMove != b.UciMove;
            var row = new ComparisonRow
            {
                MoveNumber         = b.MoveNumber,
                UciMove            = b.UciMove,
                MoveNotation       = b.MoveNotation,
                BaselineDurationMs = b.DurationMs,
                NewDurationMs      = n.DurationMs,
                BaselineTtCount    = b.TtCount,
                NewTtCount         = n.TtCount,
                BaselineMemoryMB   = b.ProcessMemoryMB,
                NewMemoryMB        = n.ProcessMemoryMB,
                IsRegression       = !diverged && isRegression,
                IsDiverged         = diverged
            };

            if (isRegression) diverged = true;
            Rows.Add(row);
        }

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        var valid = Rows.Where(r => !r.IsDiverged).ToList();

        RegressionCount = Rows.Count(r => r.IsRegression);

        if (!valid.Any())
        {
            AvgDeltaText       = "—";
            TotalTimeDeltaText = "—";
            BaselineAvg = NewAvg = BaselineMax = NewMax = BaselineTtAvg = NewTtAvg = 0;
            return;
        }

        double avgDelta = valid.Average(r => r.DeltaPercent);
        double totalDelta = valid.Sum(r => r.DeltaMs);
        double totalBaseline = valid.Sum(r => r.BaselineDurationMs);
        double totalNew = valid.Sum(r => r.NewDurationMs);

        AvgDeltaText       = $"{avgDelta:+0.0;-0.0}%";
        TotalTimeDeltaText = $"{TimeSpanDurationConverter.FormatDuration(totalNew)} vs {TimeSpanDurationConverter.FormatDuration(totalBaseline)} ({totalDelta:+0;-0} ms)";

        BaselineAvg  = valid.Average(r => r.BaselineDurationMs);
        NewAvg       = valid.Average(r => r.NewDurationMs);
        BaselineMax  = valid.Max(r => r.BaselineDurationMs);
        NewMax       = valid.Max(r => r.NewDurationMs);
        BaselineTtAvg = valid.Average(r => r.BaselineTtCount);
        NewTtAvg     = valid.Average(r => r.NewTtCount);
    }

    // ── File loading ──────────────────────────────────────────────────────
    private BenchmarkResult LoadFromDialog(string title)
    {
        var dlg = new OpenFileDialog { Title = title, Filter = "Benchmark JSON|*.json|All files|*.*" };
        if (dlg.ShowDialog() != true) return null;

        var svc = ContainerLocator.Current.Resolve<IBenchmarkFileService>();
        return svc.Load(dlg.FileName);
    }

    private void ExecuteLoadBaseline()
    {
        var r = LoadFromDialog("Open Baseline Result");
        if (r == null) return;
        Baseline = r;
        BuildRows();
    }

    private void ExecuteLoadNewRun()
    {
        var r = LoadFromDialog("Open New Run Result");
        if (r == null) return;
        NewRun = r;
        BuildRows();
    }

    private void ExecuteBack() => _navigate(new SetupViewModel(_navigate));

    // ── CSV Export ────────────────────────────────────────────────────────
    private void ExecuteExportCsv()
    {
        var dlg = new SaveFileDialog
        {
            Title  = "Export Comparison CSV",
            Filter = "CSV files|*.csv",
            FileName = $"comparison_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        if (dlg.ShowDialog() != true) return;

        var sb = new StringBuilder();
        sb.AppendLine("Move#,UCI,Notation,Status,BaselineMs,NewMs,DeltaMs,DeltaPct,BaselineTT,NewTT,DeltaTT,BaselineMemMB,NewMemMB,DeltaMemMB");
        foreach (var r in Rows)
        {
            sb.AppendLine(
                $"{r.MoveNumber},{r.UciMove},{r.MoveNotation},{r.Status}," +
                $"{r.BaselineDurationMs:F2},{r.NewDurationMs:F2},{r.DeltaMs:F2},{r.DeltaPercent:F2}," +
                $"{r.BaselineTtCount},{r.NewTtCount},{r.DeltaTtCount}," +
                $"{r.BaselineMemoryMB:F2},{r.NewMemoryMB:F2},{r.DeltaMemoryMB:F2}");
        }

        File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
    }
}
