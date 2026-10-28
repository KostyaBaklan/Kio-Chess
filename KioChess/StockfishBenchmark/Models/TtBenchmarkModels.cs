using Engine.DataStructures;

namespace StockfishBenchmark.Models;

public class TtVariant
{
    public string Name        { get; set; }
    public int    MainMB      { get; set; }   // 0 = engine default
    public int    NullMB      { get; set; }   // 0 = engine default
    public bool   AdvanceMain { get; set; }   // call ExecuteAsyncAction after every engine move
    public bool   AdvanceNull { get; set; }   // call ExecuteNullAsyncAction after every engine move
}

public class TtBenchmarkRequest
{
    public BenchmarkSession Session  { get; set; }
    public BenchmarkResult  Baseline { get; set; }   // null = play a fresh game vs Stockfish first
    public List<TtVariant>  Variants { get; set; } = new();
}

public class TtVariantResult
{
    public TtVariant Variant { get; set; }
    public string Name => Variant.Name;

    public double TotalMs        { get; set; }
    public long   Nodes          { get; set; }
    public long   QNodes         { get; set; }
    public double Knps => TotalMs <= 0 ? 0 : (Nodes + QNodes) / TotalMs;
    public int    EngineMoves    { get; set; }
    public int    MatchingMoves  { get; set; }
    public List<TtTableStats> Tables { get; set; } = new();
    public List<TtMoveRecord> Moves { get; set; } = new();

    public string Report { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;

    public double AvgMs => EngineMoves == 0 ? 0 : TotalMs / EngineMoves;

    public double MainHitPct  => HitPct("Main");
    public double NullHitPct  => HitPct("Null");
    public double MainLoadPct => LoadPct("Main");
    public double NullLoadPct => LoadPct("Null");
    public long   MainReplacements => Sum("Main", t => t.Replacements);
    public long   NullReplacements => Sum("Null", t => t.Replacements);

    private IEnumerable<TtTableStats> Of(string prefix) =>
        Tables.Where(t => t.Name.StartsWith(prefix + ".", StringComparison.Ordinal));

    private long Sum(string prefix, Func<TtTableStats, long> selector) => Of(prefix).Sum(selector);

    private double HitPct(string prefix)
    {
        long probes = Sum(prefix, t => t.Probes);
        return probes == 0 ? 0 : 100.0 * Sum(prefix, t => t.Hits) / probes;
    }

    private double LoadPct(string prefix)
    {
        long capacity = Sum(prefix, t => t.Capacity);
        return capacity == 0 ? 0 : 100.0 * Sum(prefix, t => t.Occupied) / capacity;
    }
}
