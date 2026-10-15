using Microsoft.Win32;
using StockfishBenchmark.Models;
using StockfishBenchmark.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;

namespace StockfishBenchmark.ViewModels;

public class NullRunViewModel : BindableBase
{
    private readonly Action<BindableBase> _navigate;
    private readonly BenchmarkSession _session;
    private readonly NullBenchmarkResult _baseline;
    private readonly bool _isCompare;
    private readonly NullBenchmarkRunner _runner = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _wall = new();
    private readonly List<NullMoveRecord> _all = new();

    public ObservableCollection<NullMoveRecord> LiveMoves { get; } = new();

    private string _title;
    public string Title { get => _title; set => SetProperty(ref _title, value); }

    private string _statusText = "Ready";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private string _elapsedText = "";
    public string ElapsedText { get => _elapsedText; set => SetProperty(ref _elapsedText, value); }

    private string _summaryText = "";
    public string SummaryText { get => _summaryText; set => SetProperty(ref _summaryText, value); }

    private string _warningText = "";
    public string WarningText { get => _warningText; set => SetProperty(ref _warningText, value); }

    private NullBenchmarkResult _result;
    public bool CanSave => _result != null;

    public ICommand CancelCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand BackCommand { get; }

    public NullRunViewModel(BenchmarkSession session, Action<BindableBase> navigate)
        : this(session, null, navigate) { }

    public NullRunViewModel(NullBenchmarkResult baseline, Action<BindableBase> navigate)
        : this(baseline.Session, baseline, navigate) { }

    private NullRunViewModel(BenchmarkSession session, NullBenchmarkResult baseline, Action<BindableBase> navigate)
    {
        _session = session;
        _baseline = baseline;
        _isCompare = baseline != null;
        _navigate = navigate;
        Title = _isCompare ? "Null Benchmark - compare with baseline" : "Null Benchmark - run vs Stockfish";

        CancelCommand = new DelegateCommand(() => _cts.Cancel());
        SaveCommand = new DelegateCommand(Save, () => CanSave);
        BackCommand = new DelegateCommand(() => { _cts.Cancel(); _navigate(new SetupViewModel(_navigate)); });

        if (!Engine.Strategies.Base.NullStats.Enabled)
            WarningText = "NULL_STATS is not defined for the Engine project: null counters will be zero. " +
                          "Add NULL_STATS to Engine conditional compilation symbols and rebuild.";

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _timer.Tick += (_, _) => ElapsedText = _wall.Elapsed.ToString(@"mm\:ss\.fff");

        Start();
    }

    private void Start()
    {
        StatusText = "Running...";
        _wall.Restart();
        _timer.Start();

        var progress = new Progress<NullMoveRecord>(r =>
        {
            _all.Add(r);
            LiveMoves.Add(r);
            if (r.IsEngineMove)
                SummaryText = NullBenchmarkSummary.Compute(_all, _isCompare).ToText();
        });

        var task = _isCompare
            ? _runner.CompareAsync(_baseline, progress, _cts.Token)
            : _runner.RunAsync(_session, progress, _cts.Token);

        task.ContinueWith(t =>
        {
            _timer.Stop();
            _wall.Stop();
            if (t.IsCanceled)
            {
                StatusText = "Cancelled";
                _result = Partial();
            }
            else if (t.IsFaulted)
            {
                StatusText = "Error: " + (t.Exception?.InnerException?.Message ?? t.Exception?.Message);
            }
            else
            {
                _result = t.Result;
                StatusText = string.IsNullOrEmpty(_result.EndReason) ? "Complete" : "Complete - " + _result.EndReason;
            }

            if (_result != null)
                SummaryText = _result.Summary.ToText();

            RaisePropertyChanged(nameof(CanSave));
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private NullBenchmarkResult Partial() => new()
    {
        NullStatsEnabled = Engine.Strategies.Base.NullStats.Enabled,
        Session = _session,
        Moves = _all,
        Summary = NullBenchmarkSummary.Compute(_all, _isCompare)
    };

    private void Save()
    {
        if (_result == null) return;
        var defaultPath = NullBenchmarkRunner.GetDefaultPath(_result.Session);
        var dlg = new SaveFileDialog
        {
            Title = "Save Null Benchmark",
            Filter = "Null benchmark JSON|*.json",
            FileName = Path.GetFileName(defaultPath),
            InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(defaultPath))
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            NullBenchmarkRunner.Save(_result, dlg.FileName);
            StatusText = "Saved -> " + dlg.FileName;
        }
        catch (Exception ex)
        {
            StatusText = "Save failed: " + ex.Message;
        }
    }
}
