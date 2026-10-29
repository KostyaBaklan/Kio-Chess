using Microsoft.Win32;
using StockfishBenchmark.Models;
using StockfishBenchmark.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace StockfishBenchmark.ViewModels;

public class TtRunViewModel : BindableBase
{
    private readonly Action<BindableBase> _navigate;
    private readonly TtBenchmarkRequest _request;
    private readonly CancellationTokenSource _cts = new();
    private readonly IProgress<string> _status;
    private readonly IProgress<TtVariantResult> _completed;
    private readonly IProgress<TtMoveRecord> _moves;

    public ObservableCollection<TtMoveRecord> Moves { get; } = new();

    private TtMoveRecord _selectedMove;
    public TtMoveRecord SelectedMove
    {
        get => _selectedMove;
        set
        {
            if (SetProperty(ref _selectedMove, value))
            {
                RaisePropertyChanged(nameof(ReportText));
                RaisePropertyChanged(nameof(AdviceText));
            }
        }
    }

    private string _liveText = string.Empty;
    public string LiveText { get => _liveText; set => SetProperty(ref _liveText, value); }

    public ObservableCollection<TtVariantResult> Results { get; } = new();

    public TtDashboardViewModel Dashboard { get; } = new();

    private string _statusText = "Starting...";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private bool _isRunning = true;
    public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }

    private TtVariantResult _selectedResult;
    public TtVariantResult SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (SetProperty(ref _selectedResult, value))
            {
                RaisePropertyChanged(nameof(ReportText));
                RaisePropertyChanged(nameof(AdviceText));
            }
        }
    }

    public string ReportText => SelectedMove != null && !string.IsNullOrEmpty(SelectedMove.Report) ? SelectedMove.Report : SelectedResult?.Report ?? string.Empty;
    public string AdviceText => SelectedMove != null && !string.IsNullOrEmpty(SelectedMove.Advice) ? SelectedMove.Advice : SelectedResult?.Advice ?? string.Empty;

    public ICommand CancelCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SaveReportCommand { get; }

    public TtRunViewModel(TtBenchmarkRequest request, Action<BindableBase> navigate)
    {
        _request = request;
        _navigate = navigate;

        _status = new Progress<string>(s => StatusText = s);
        _completed = new Progress<TtVariantResult>(r =>
        {
            Results.Add(r);
            Dashboard.Add(r);
            SelectedResult ??= r;
        });

        _moves = new Progress<TtMoveRecord>(m =>
        {
            Moves.Add(m);
            SelectedMove = m;
            LiveText = $"{m.VariantName} | ply {m.MoveNumber} {m.Player} {m.UciMove} | process {m.ProcessMemoryMB:F0} MB, managed {m.ManagedMemoryMB:F0} MB | " +
                       $"main gen {m.MainGeneration} hit {m.MainHitPct:F1}% load {m.MainLoadPct:F1}% repl {m.MainReplacements} | " +
                       $"null gen {m.NullGeneration} hit {m.NullHitPct:F1}% load {m.NullLoadPct:F1}% repl {m.NullReplacements}";
        });

        CancelCommand     = new DelegateCommand(() => _cts.Cancel());
        BackCommand       = new DelegateCommand(() => { _cts.Cancel(); _navigate(new StartViewModel(_navigate)); });
        SaveReportCommand = new DelegateCommand(SaveReport);

        _ = RunAsync();
    }

    private async Task RunAsync()
    {
        try
        {
            var baseline = _request.Baseline;
            if (baseline == null)
            {
                _status.Report("Playing the reference game against Stockfish...");
                var runner = ContainerLocator.Current.Resolve<IBenchmarkRunner>();
                baseline = await runner.RunAsync(
                    _request.Session,
                    new Progress<BenchmarkMoveRecord>(m => StatusText = $"Reference game: ply {m.MoveNumber} {m.UciMove}"),
                    _cts.Token);
            }

            var tt = ContainerLocator.Current.Resolve<ITtBenchmarkRunner>();
            await tt.RunAsync(baseline, _request.Variants, _status, _completed, _moves, _cts.Token);
            StatusText = $"Done: {Results.Count} variant(s).";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Cancelled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed: {ex.Message}";
            LiveText = ex.ToString();
            try { File.WriteAllText(Path.Combine(Path.GetTempPath(), "tt_benchmark_error.txt"), ex.ToString()); } catch { }
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void SaveReport()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Save TT statistics report",
            Filter = "Text|*.txt",
            FileName = $"tt_stats_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };
        if (dlg.ShowDialog() != true) return;

        var sb = new StringBuilder();
        sb.AppendLine("Variant summary");
        sb.AppendLine("variant | ms | avg ms | same moves | main hit% | main load% | main repl | null hit% | null load% | null repl");
        foreach (var r in Results)
        {
            sb.AppendLine($"{r.Name} | {r.TotalMs:F0} | {r.AvgMs:F0} | {r.MatchingMoves}/{r.EngineMoves} | {r.MainHitPct:F1} | {r.MainLoadPct:F1} | {r.MainReplacements} | {r.NullHitPct:F1} | {r.NullLoadPct:F1} | {r.NullReplacements}");
        }
        foreach (var r in Results)
        {
            sb.AppendLine();
            sb.AppendLine($"Moves of {r.Name}");
            sb.AppendLine("ply | player | move | same | ms | process MB | managed MB | main gen | main hit% | main load% | main repl | null gen | null hit% | null load% | null repl");
            foreach (var m in r.Moves)
                sb.AppendLine($"{m.MoveNumber} | {m.Player} | {m.UciMove} | {m.MatchesBaseline} | {m.DurationMs:F0} | {m.ProcessMemoryMB:F0} | {m.ManagedMemoryMB:F0} | {m.MainGeneration} | {m.MainHitPct:F1} | {m.MainLoadPct:F1} | {m.MainReplacements} | {m.NullGeneration} | {m.NullHitPct:F1} | {m.NullLoadPct:F1} | {m.NullReplacements}");
        }
        foreach (var r in Results)
        {
            sb.AppendLine();
            sb.AppendLine(r.Report);
            sb.AppendLine(r.Advice);
        }
        File.WriteAllText(dlg.FileName, sb.ToString());
    }
}
