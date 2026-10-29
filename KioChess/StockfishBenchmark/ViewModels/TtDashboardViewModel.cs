using Engine.DataStructures;
using StockfishBenchmark.Controls;
using StockfishBenchmark.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace StockfishBenchmark.ViewModels;

/// <summary>Results of every TT run in this application session, so several depths can be compared in one dashboard.</summary>
public static class TtDashboardHistory
{
    public static List<TtDashboardRow> Rows { get; } = new();
}

public class TtDashboardViewModel : BindableBase
{
    private static readonly string[] VictimSeries =
        ["shallowest+oldest", "shallowest only (depth rule)", "oldest only (age rule)", "neither (depth/age conflict)"];

    public ObservableCollection<TtDashboardRow> Rows { get; } = new();

    public string[] HeatSources { get; } = ["Victims", "Hits", "Residents"];
    private string _heatSource = "Victims";
    public string HeatSource
    {
        get => _heatSource;
        set { if (SetProperty(ref _heatSource, value)) RefreshHeat(); }
    }

    private TtDashboardRow _selectedRow;
    public TtDashboardRow SelectedRow
    {
        get => _selectedRow;
        set { if (SetProperty(ref _selectedRow, value)) RefreshHeat(); }
    }

    private long[] _heat = [];
    public long[] Heat { get => _heat; private set => SetProperty(ref _heat, value); }

    private string _heatTitle = string.Empty;
    public string HeatTitle { get => _heatTitle; private set => SetProperty(ref _heatTitle, value); }

    private IReadOnlyList<TtBarItem> _hitChart = [];
    public IReadOnlyList<TtBarItem> HitChart { get => _hitChart; private set => SetProperty(ref _hitChart, value); }

    private IReadOnlyList<TtBarItem> _timeChart = [];
    public IReadOnlyList<TtBarItem> TimeChart { get => _timeChart; private set => SetProperty(ref _timeChart, value); }

    private IReadOnlyList<TtBarItem> _memoryChart = [];
    public IReadOnlyList<TtBarItem> MemoryChart { get => _memoryChart; private set => SetProperty(ref _memoryChart, value); }

    private IReadOnlyList<TtBarItem> _victimChart = [];
    public IReadOnlyList<TtBarItem> VictimChart { get => _victimChart; private set => SetProperty(ref _victimChart, value); }
    public IReadOnlyList<string> VictimSeriesNames => VictimSeries;

    private IReadOnlyList<TtBarItem> _residentDepthChart = [];
    public IReadOnlyList<TtBarItem> ResidentDepthChart { get => _residentDepthChart; private set => SetProperty(ref _residentDepthChart, value); }

    private IReadOnlyList<TtBarItem> _staleChart = [];
    public IReadOnlyList<TtBarItem> StaleChart { get => _staleChart; private set => SetProperty(ref _staleChart, value); }

    private IReadOnlyList<TtBarItem> _victimDepthChart = [];
    public IReadOnlyList<TtBarItem> VictimDepthChart { get => _victimDepthChart; private set => SetProperty(ref _victimDepthChart, value); }

    private IReadOnlyList<TtBarItem> _churnChart = [];
    public IReadOnlyList<TtBarItem> ChurnChart { get => _churnChart; private set => SetProperty(ref _churnChart, value); }

    private string _conclusions = "No results yet.";
    public string Conclusions { get => _conclusions; private set => SetProperty(ref _conclusions, value); }

    public ICommand ClearHistoryCommand { get; }

    public TtDashboardViewModel()
    {
        ClearHistoryCommand = new DelegateCommand(() =>
        {
            TtDashboardHistory.Rows.Clear();
            Rebuild();
        });
        Rebuild();
    }

    public void Add(TtVariantResult result)
    {
        TtDashboardHistory.Rows.Add(new TtDashboardRow(result));
        Rebuild();
    }

    private void Rebuild()
    {
        var ordered = TtDashboardHistory.Rows
            .OrderBy(r => r.SearchDepth).ThenBy(r => r.MemoryMB).ThenBy(r => r.DepthFactor).ToList();

        Rows.Clear();
        foreach (var r in ordered) Rows.Add(r);

        HitChart = Bars(ordered, r => r.MainHit);
        TimeChart = Bars(ordered, r => r.AvgMs);
        MemoryChart = Bars(ordered, r => r.MemoryMB);
        VictimChart = ordered.Select(r => new TtBarItem
        {
            Label = r.Label,
            Values = [r.ShallowestOldestPct, r.ShallowestOnlyPct, r.OldestOnlyPct, r.NeitherPct]
        }).ToList();
        ResidentDepthChart = Bars(ordered, r => r.AvgResidentDepth);
        StaleChart = Bars(ordered, r => r.StaleResidentPct);
        VictimDepthChart = Bars(ordered, r => r.AvgVictimDepth);
        ChurnChart = Bars(ordered, r => r.GenerationChurnPct);

        SelectedRow = ordered.LastOrDefault();
        Conclusions = BuildConclusions(ordered);
    }

    private static List<TtBarItem> Bars(IEnumerable<TtDashboardRow> rows, Func<TtDashboardRow, double> value) =>
        rows.Select(r => new TtBarItem { Label = r.Label, Values = [value(r)] }).ToList();

    private void RefreshHeat()
    {
        if (SelectedRow == null)
        {
            Heat = [];
            HeatTitle = string.Empty;
            return;
        }
        Heat = SelectedRow.Heat("Main", HeatSource);
        HeatTitle = $"Main table {HeatSource.ToLowerInvariant()}: depth x age - {SelectedRow.Label}";
    }

    private static string BuildConclusions(List<TtDashboardRow> rows)
    {
        if (rows.Count == 0) return "No results yet.";
        var sb = new StringBuilder();

        foreach (var group in rows.GroupBy(r => r.SearchDepth).OrderBy(g => g.Key))
        {
            var list = group.ToList();
            sb.AppendLine($"Depth {group.Key}: {list.Count} variant(s)");

            var fastest = list.MinBy(r => r.AvgMs);
            var bestHit = list.MaxBy(r => r.MainHit);
            sb.AppendLine($"  fastest: {fastest.Name} ({fastest.AvgMs:F0} ms/move, {fastest.MemoryMB:F0} MB)");
            sb.AppendLine($"  best main hit rate: {bestHit.Name} ({bestHit.MainHit:F1}%)");

            var cheap = list.Where(r => r.AvgMs <= fastest.AvgMs * 1.03).MinBy(r => r.MemoryMB);
            sb.AppendLine($"  smallest memory within 3% of fastest: {cheap.Name} ({cheap.MemoryMB:F0} MB, {cheap.AvgMs:F0} ms/move)");

            var withRepl = list.Where(r => r.MainRepl > 0).ToList();
            if (withRepl.Count > 0)
            {
                double depthDecided = withRepl.Average(r => r.DepthDecidedPct);
                double ageOverride = withRepl.Average(r => r.GenerationOverridePct);
                sb.AppendLine($"  victim chosen by depth rule {depthDecided:F0}% / age rule or conflict {ageOverride:F0}% (averaged over variants with replacements)");
                if (ageOverride < 5)
                    sb.AppendLine("  -> depth term dominates: depth factor is large relative to generation, age barely matters.");
                else if (ageOverride > 35)
                    sb.AppendLine("  -> generation overrides depth often: deep entries may be evicted by age, consider a larger depth factor.");
                else
                    sb.AppendLine("  -> depth and age both take part in the decision.");
            }

            var stale = list.Where(r => r.Generations > 0).ToList();
            if (stale.Count > 0)
            {
                var s = stale.MaxBy(r => r.StaleResidentPct);
                sb.AppendLine($"  stalest table: {s.Name} ({s.StaleResidentPct:F0}% of residents older than 3 generations, avg age {s.AvgResidentAge:F1})");
                sb.AppendLine($"  generations per engine move: {stale.Average(r => r.GenerationsPerMove):F2}, stores per generation: {stale.Average(r => r.StoresPerGeneration):F0}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Reading guide: low depth-rule share and high stale share mean old deep entries are never evicted; ");
        sb.AppendLine("high victim-deeper-than-new means useful deep entries are lost. Compare depth factors at equal table sizes.");
        return sb.ToString();
    }
}
