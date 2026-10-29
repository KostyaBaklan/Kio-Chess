using Engine.DataStructures;

namespace StockfishBenchmark.Models;

/// <summary>Flat, display-ready summary of one variant result (main-table replacement analysis included).</summary>
public class TtDashboardRow
{
    private const int BucketBytes = 64;
    private const int StaleAge = 3;

    public TtVariantResult Result { get; }

    public string Name => Result.Name;
    public int SearchDepth => Result.Variant.SearchDepth;
    public string Label => SearchDepth > 0 ? $"D{SearchDepth} {Name}" : Name;

    public int DepthFactor { get; }
    public string DepthFactorText => DepthFactor < 0 ? "-" : DepthFactor.ToString();
    public int DepthWeight => DepthFactor < 0 ? 0 : 1 << DepthFactor;

    public double MemoryMB { get; }
    public double TotalMs => Result.TotalMs;
    public double AvgMs => Result.AvgMs;
    public double Knps => Result.Knps;
    public long Nodes => Result.Nodes + Result.QNodes;
    public double SamePct => Result.EngineMoves == 0 ? 0 : 100.0 * Result.MatchingMoves / Result.EngineMoves;

    public double MainHit => Result.MainHitPct;
    public double MainLoad => Result.MainLoadPct;
    public long MainRepl => Result.MainReplacements;
    public double NullHit => Result.NullHitPct;
    public double NullLoad => Result.NullLoadPct;
    public long NullRepl => Result.NullReplacements;

    // Main table replacement analysis: which part of the priority chose the victim
    public double ShallowestOldestPct { get; }
    public double ShallowestOnlyPct { get; }
    public double OldestOnlyPct { get; }
    public double NeitherPct { get; }
    public double DepthDecidedPct => ShallowestOldestPct + ShallowestOnlyPct;
    public double GenerationOverridePct => OldestOnlyPct + NeitherPct;
    public double VictimDeeperThanNewPct { get; }
    public double AvgVictimDepth { get; }
    public double AvgNewDepth { get; }
    public double AvgDepthSpread { get; }
    public double AvgGenSpread { get; }
    public double AvgPriorityMargin { get; }
    public double AvgVictimAge { get; }
    public double VictimCurrentGenPct { get; }

    // Main table content / usefulness
    public double AvgResidentDepth { get; }
    public double AvgResidentAge { get; }
    public double StaleResidentPct { get; }
    public double DeepResidentPct { get; }
    public double AvgHitAge { get; }
    public double AvgHitDepth { get; }

    // Generation cadence (how often the age term moves relative to table size)
    public int Generations { get; }
    public double StoresPerGeneration { get; }
    public double GenerationChurnPct { get; }
    public double GenerationsPerMove { get; }

    public TtDashboardRow(TtVariantResult result)
    {
        Result = result;
        var all = result.Tables;
        var main = TtAggregate.Of(all, "Main").ToList();

        DepthFactor = main.Select(t => t.DepthFactor).DefaultIfEmpty(-1).First();
        MemoryMB = all.Sum(t => t.Buckets) * (double)BucketBytes / 1_048_576.0;

        long repl = main.Sum(t => t.Replacements);
        ShallowestOldestPct = Pct(main.Sum(t => t.VictimShallowestAndOldest), repl);
        ShallowestOnlyPct = Pct(main.Sum(t => t.VictimShallowestOnly), repl);
        OldestOnlyPct = Pct(main.Sum(t => t.VictimOldestOnly), repl);
        NeitherPct = Pct(main.Sum(t => t.VictimNeither), repl);
        VictimDeeperThanNewPct = Pct(main.Sum(t => t.VictimDeeperThanNew), repl);
        AvgVictimDepth = Ratio(main.Sum(t => t.VictimDepthSum), repl);
        AvgNewDepth = Ratio(main.Sum(t => t.NewDepthSum), repl);
        AvgDepthSpread = Ratio(main.Sum(t => t.DepthSpreadSum), repl);
        AvgGenSpread = Ratio(main.Sum(t => t.GenSpreadSum), repl);
        AvgPriorityMargin = Ratio(main.Sum(t => t.PriorityMarginSum), repl);
        VictimCurrentGenPct = Pct(main.Sum(t => t.VictimCurrentGeneration), repl);
        AvgVictimAge = WeightedIndex(Sum(main, t => t.VictimAge));

        var depthHist = Sum(main, t => t.DepthHistogram);
        var ageHist = Sum(main, t => t.AgeHistogram);
        long residents = depthHist.Sum();
        AvgResidentDepth = WeightedIndex(depthHist);
        AvgResidentAge = WeightedIndex(ageHist);
        StaleResidentPct = Pct(ageHist.Skip(StaleAge).Sum(), residents);
        int deepFrom = Math.Max(1, result.Variant.SearchDepth - 2);
        DeepResidentPct = Pct(depthHist.Skip(deepFrom).Sum(), residents);
        AvgHitAge = WeightedIndex(Sum(main, t => t.HitAge));
        AvgHitDepth = WeightedIndex(Sum(main, t => t.HitDepth));

        Generations = main.Select(t => t.Generation).DefaultIfEmpty(0).Max();
        long stores = main.Sum(t => t.Stores);
        long capacity = main.Sum(t => t.Capacity);
        StoresPerGeneration = Ratio(stores, Generations + 1);
        GenerationChurnPct = Pct(StoresPerGeneration, capacity);
        GenerationsPerMove = Ratio(Generations, result.EngineMoves);
    }

    public long[] Heat(string prefix, string source)
    {
        var tables = TtAggregate.Of(Result.Tables, prefix).ToList();
        return source switch
        {
            "Hits" => Sum(tables, t => t.HitDepthAge),
            "Residents" => Sum(tables, t => t.ResidentDepthAge),
            _ => Sum(tables, t => t.VictimDepthAge)
        };
    }

    private static long[] Sum(IEnumerable<TtTableStats> tables, Func<TtTableStats, long[]> selector)
    {
        long[] total = null;
        foreach (var t in tables)
        {
            var a = selector(t);
            if (a == null) continue;
            total ??= new long[a.Length];
            for (int i = 0; i < a.Length && i < total.Length; i++) total[i] += a[i];
        }
        return total ?? [];
    }

    private static double WeightedIndex(long[] bins)
    {
        long total = bins.Sum();
        if (total == 0) return 0;
        double sum = 0;
        for (int i = 0; i < bins.Length; i++) sum += (double)i * bins[i];
        return sum / total;
    }

    private static double Pct(double a, double b) => b <= 0 ? 0 : 100.0 * a / b;
    private static double Ratio(double a, double b) => b <= 0 ? 0 : a / b;
}
