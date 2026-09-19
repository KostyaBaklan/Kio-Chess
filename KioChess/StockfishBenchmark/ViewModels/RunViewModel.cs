using Microsoft.Win32;
using StockfishBenchmark.Converters;
using StockfishBenchmark.Models;
using StockfishBenchmark.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;

namespace StockfishBenchmark.ViewModels;

public class RunViewModel : BindableBase
{
    private readonly Action<BindableBase> _navigate;
    private readonly BenchmarkSession    _session;         // set for Mode A
    private readonly BenchmarkResult     _baseline;        // set for Mode B (replay)
    private readonly bool                _isReplay;

    private CancellationTokenSource _cts;
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _wallClock = new();
    private readonly List<BenchmarkMoveRecord> _allMoves = new List<BenchmarkMoveRecord>();

    // ── Live move list ─────────────────────────────────────────────────────
    public ObservableCollection<BenchmarkMoveRecord> LiveMoves { get; } = new();

    // ── Status / progress ──────────────────────────────────────────────────
    private string _statusText = "Ready";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private double _progressPercent;
    public double ProgressPercent
    {
        get => _progressPercent;
        set => SetProperty(ref _progressPercent, value);
    }

    private string _elapsedText = "00:00:000.000";
    public string ElapsedText
    {
        get => _elapsedText;
        set => SetProperty(ref _elapsedText, value);
    }

    // ── Live duration stats ────────────────────────────────────────────────
        // -- Notification ----------------------------------------------------------
    private string _notificationText;
    public string NotificationText
    {
        get => _notificationText;
        set => SetProperty(ref _notificationText, value);
    }

    private bool _isSuccessNotificationVisible;
    public bool IsSuccessNotificationVisible
    {
        get => _isSuccessNotificationVisible;
        set => SetProperty(ref _isSuccessNotificationVisible, value);
    }

    private bool _isWarningNotificationVisible;
    public bool IsWarningNotificationVisible
    {
        get => _isWarningNotificationVisible;
        set => SetProperty(ref _isWarningNotificationVisible, value);
    }

    private void ShowSuccessNotification(string message)
    {
        NotificationText             = message;
        IsSuccessNotificationVisible = true;
        IsWarningNotificationVisible = false;
    }

    private void ShowWarningNotification(string message)
    {
        NotificationText             = message;
        IsSuccessNotificationVisible = false;
        IsWarningNotificationVisible = true;
    }
private string _avgDurationText = "—";
    public string AvgDurationText
    {
        get => _avgDurationText;
        set => SetProperty(ref _avgDurationText, value);
    }

    private string _minDurationText = "—";
    public string MinDurationText
    {
        get => _minDurationText;
        set => SetProperty(ref _minDurationText, value);
    }

    private string _maxDurationText = "—";
    public string MaxDurationText
    {
        get => _maxDurationText;
        set => SetProperty(ref _maxDurationText, value);
    }

    // ── Baseline duration stats (until current move) ────────────────────────
    private string _baselineAvgDurationText = "—";
    public string BaselineAvgDurationText
    {
        get => _baselineAvgDurationText;
        set => SetProperty(ref _baselineAvgDurationText, value);
    }

    private string _baselineMinDurationText = "—";
    public string BaselineMinDurationText
    {
        get => _baselineMinDurationText;
        set => SetProperty(ref _baselineMinDurationText, value);
    }

    private string _baselineMaxDurationText = "—";
    public string BaselineMaxDurationText
    {
        get => _baselineMaxDurationText;
        set => SetProperty(ref _baselineMaxDurationText, value);
    }

    // ── Result ────────────────────────────────────────────────────────────
    private BenchmarkResult _currentResult;
    public BenchmarkResult CurrentResult
    {
        get => _currentResult;
        private set
        {
            SetProperty(ref _currentResult, value);
            RaisePropertyChanged(nameof(CanSave));
            RaisePropertyChanged(nameof(CanCompare));
        }
    }

    public bool CanSave    => CurrentResult != null;
    public bool CanCompare => CurrentResult != null;

    // ── Commands ──────────────────────────────────────────────────────────
    public ICommand CancelCommand  { get; }
    public ICommand SaveCommand    { get; }
    public ICommand CompareCommand { get; }
    public ICommand BackCommand    { get; }

    // ── Mode A constructor (new run) ──────────────────────────────────────
    public RunViewModel(BenchmarkSession session, Action<BindableBase> navigate)
    {
        _session  = session;
        _navigate = navigate;
        _isReplay = false;

        CancelCommand  = new DelegateCommand(ExecuteCancel);
        SaveCommand    = new DelegateCommand(ExecuteSave,    () => CanSave);
        CompareCommand = new DelegateCommand(ExecuteCompare, () => CanCompare);
        BackCommand    = new DelegateCommand(ExecuteBack);

        _timer = BuildTimer();
        StartRun();
    }

    // ── Mode B constructor (replay) ────────────────────────────────────────
    public RunViewModel(BenchmarkResult baseline, Action<BindableBase> navigate)
    {
        _baseline = baseline;
        _session  = baseline.Session;
        _navigate = navigate;
        _isReplay = true;

        CancelCommand  = new DelegateCommand(ExecuteCancel);
        SaveCommand    = new DelegateCommand(ExecuteSave,    () => CanSave);
        CompareCommand = new DelegateCommand(ExecuteCompare, () => CanCompare);
        BackCommand    = new DelegateCommand(ExecuteBack);

        _timer = BuildTimer();
        StartReplay();
    }

    // ── Timer ─────────────────────────────────────────────────────────────
    private DispatcherTimer BuildTimer()
    {
        var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        t.Tick += (_, _) =>
        {
            ElapsedText = TimeSpanDurationConverter.FormatDuration(_wallClock.Elapsed);
        };
        return t;
    }

    // ── Mode A: fresh run ─────────────────────────────────────────────────
    private void StartRun()
    {
        _cts = new CancellationTokenSource();
        StatusText = "Running…";
        _wallClock.Restart();
        _timer.Start();

        int targetMoves = _session.MoveCount;
        var progress = new Progress<BenchmarkMoveRecord>(r => OnMoveRecorded(r, targetMoves));

        var runner = ContainerLocator.Current.Resolve<IBenchmarkRunner>();
        runner.RunAsync(_session, progress, _cts.Token)
              .ContinueWith(OnRunComplete, TaskScheduler.FromCurrentSynchronizationContext());
    }

    // ── Mode B: replay ────────────────────────────────────────────────────
    private void StartReplay()
    {
        _cts = new CancellationTokenSource();
        StatusText = "Replaying…";
        _wallClock.Restart();
        _timer.Start();

        int targetMoves = _baseline.Session.MoveCount;
        var replayProgress = new Progress<ComparisonRow>(row =>
        {
            // Convert ComparisonRow to BenchmarkMoveRecord for the live list
            var rec = new BenchmarkMoveRecord
            {
                MoveNumber      = row.MoveNumber,
                IsEngineMove    = row.BaselineDurationMs > 0,
                UciMove         = row.UciMove,
                MoveNotation    = row.MoveNotation,
                DurationMs      = row.NewDurationMs,
                TtCount         = row.NewTtCount,
                ProcessMemoryMB = row.NewMemoryMB
            };
            OnMoveRecorded(rec, targetMoves);
        });

        var replayRunner = ContainerLocator.Current.Resolve<IReplayRunner>();
        replayRunner.CompareAsync(_baseline, replayProgress, _cts.Token)
                    .ContinueWith(OnReplayComplete, TaskScheduler.FromCurrentSynchronizationContext());
    }

    // ── Progress callback (UI thread) ─────────────────────────────────────
    private void OnMoveRecorded(BenchmarkMoveRecord record, int targetMoves)
    {
        _allMoves.Add(record);
        if (record.IsEngineMove)
        {
            LiveMoves.Add(record); 
        }

        var engineMoves = LiveMoves.ToList();
        if (engineMoves.Count > 0)
        {
            static string Fmt(double ms) => TimeSpanDurationConverter.FormatDuration(ms);
            AvgDurationText = Fmt(engineMoves.Average(m => m.DurationMs));
            MinDurationText = Fmt(engineMoves.Min(m => m.DurationMs));
            MaxDurationText = Fmt(engineMoves.Max(m => m.DurationMs));
            ProgressPercent = Math.Min(100.0, engineMoves.Count * 100.0 / targetMoves);

            // ── Update baseline stats during replay ──────────────────────────
            if (_isReplay && _baseline?.Moves != null)
            {
                var baselineMoves = _baseline.Moves
                    .Where(m => m.IsEngineMove)
                    .Take(engineMoves.Count)
                    .ToList();

                if (baselineMoves.Count > 0)
                {
                    BaselineAvgDurationText = Fmt(baselineMoves.Average(m => m.DurationMs));
                    BaselineMinDurationText = Fmt(baselineMoves.Min(m => m.DurationMs));
                    BaselineMaxDurationText = Fmt(baselineMoves.Max(m => m.DurationMs));
                }
            }
        }
    }

    // ── Completion callbacks ──────────────────────────────────────────────
    private void OnRunComplete(Task<BenchmarkResult> task)
    {
        _timer.Stop();
        _wallClock.Stop();

        if (task.IsCanceled)
        {
            StatusText = "Cancelled";
            CurrentResult = BuildPartialResult();
            ShowWarningNotification($"⚠  Run cancelled — partial results available  ·  Elapsed: {ElapsedText}");
        }
        else if (task.IsFaulted)
        {
            var msg = task.Exception?.InnerException?.Message ?? task.Exception?.Message;
            StatusText = $"Error: {msg}";
            ShowWarningNotification($"✖  Error: {msg}");
        }
        else
        {
            CurrentResult = task.Result;
            int n = CurrentResult.Moves.Count(m => m.IsEngineMove);
            StatusText = $"Complete — {n} engine moves recorded";
            ProgressPercent = 100;
            ShowSuccessNotification($"✔  Benchmark complete — {n} engine moves  ·  Elapsed: {ElapsedText}");
        }
        ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        ((DelegateCommand)CompareCommand).RaiseCanExecuteChanged();
    }

    private void OnReplayComplete(Task<List<ComparisonRow>> task)
    {
        _timer.Stop();
        _wallClock.Stop();

        if (task.IsCanceled)
        {
            StatusText = "Cancelled";
            CurrentResult = BuildPartialResult();
            ShowWarningNotification($"⚠  Replay cancelled — partial results available  ·  Elapsed: {ElapsedText}");
        }
        else if (task.IsFaulted)
        {
            var msg = task.Exception?.InnerException?.Message ?? task.Exception?.Message;
            StatusText = $"Error: {msg}";
            ShowWarningNotification($"✖  Error: {msg}");
        }
        else
        {
            CurrentResult = BuildPartialResult();
            CurrentResult.Summary = BenchmarkResult.ComputeSummary(CurrentResult.Moves);
            int regressions = task.Result.Count(r => r.IsRegression);
            StatusText = $"Replay complete — {regressions} regression(s)";
            ProgressPercent = 100;
            string regressText = regressions == 0 ? "no regressions" : $"{regressions} regression(s)";
            ShowSuccessNotification($"✔  Replay complete — {regressText}  ·  Elapsed: {ElapsedText}");
        }
        ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        ((DelegateCommand)CompareCommand).RaiseCanExecuteChanged();
    }

    private BenchmarkResult BuildPartialResult() => new()
    {
        Session = _session,
        Moves   = _allMoves,
        Summary = BenchmarkResult.ComputeSummary(_allMoves)
    };

    // ── Commands ──────────────────────────────────────────────────────────
    private void ExecuteCancel() => _cts?.Cancel();

    private void ExecuteSave()
    {
        if (CurrentResult == null) return;

        var svc         = ContainerLocator.Current.Resolve<IBenchmarkFileService>();
        var defaultPath = svc.GetDefaultPath(CurrentResult.Session);

        var dlg = new SaveFileDialog
        {
            Title      = "Save Benchmark Result",
            Filter     = "Benchmark JSON|*.json",
            FileName   = Path.GetFileName(defaultPath),
            InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(defaultPath))
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            svc.Save(CurrentResult, dlg.FileName);
            StatusText = $"Saved → {dlg.FileName}";
        }
        catch (Exception ex)
        {
            StatusText = $"Save failed: {ex.Message}";
        }
    }

    private void ExecuteCompare()
    {
        if (CurrentResult == null) return;

        BenchmarkResult baseline = _isReplay ? _baseline : CurrentResult;
        BenchmarkResult newRun   = CurrentResult;

        _navigate(new CompareViewModel(baseline, newRun, _navigate));
    }

    private void ExecuteBack() => _navigate(new SetupViewModel(_navigate));
}
