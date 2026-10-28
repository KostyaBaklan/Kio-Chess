using Engine.DataStructures;

namespace StockfishBenchmark.Models;

public static class TtAggregate
{
    public static IEnumerable<TtTableStats> Of(IEnumerable<TtTableStats> tables, string prefix) =>
        tables.Where(t => t.Name.StartsWith(prefix + ".", StringComparison.Ordinal));

    public static long Sum(IEnumerable<TtTableStats> tables, string prefix, Func<TtTableStats, long> selector) =>
        Of(tables, prefix).Sum(selector);

    public static double HitPct(IEnumerable<TtTableStats> tables, string prefix)
    {
        long probes = Sum(tables, prefix, t => t.Probes);
        return probes == 0 ? 0 : 100.0 * Sum(tables, prefix, t => t.Hits) / probes;
    }

    public static double LoadPct(IEnumerable<TtTableStats> tables, string prefix)
    {
        long capacity = Sum(tables, prefix, t => t.Capacity);
        return capacity == 0 ? 0 : 100.0 * Sum(tables, prefix, t => t.Occupied) / capacity;
    }

    public static int Generation(IEnumerable<TtTableStats> tables, string prefix) =>
        Of(tables, prefix).Select(t => t.Generation).DefaultIfEmpty(0).Max();
}

public class TtMoveRecord
{
    public string VariantName { get; set; }
    public int    MoveNumber  { get; set; }
    public bool   IsEngineMove { get; set; }
    public string Player => IsEngineMove ? "Engine" : "Stockfish";
    public string UciMove { get; set; }
    public string MoveNotation { get; set; }
    public bool   MatchesBaseline { get; set; }
    public double DurationMs { get; set; }
    public long   Nodes { get; set; }
    public long   QNodes { get; set; }
    public double Knps => DurationMs <= 0 ? 0 : (Nodes + QNodes) / DurationMs;
    public double ProcessMemoryMB { get; set; }
    public double ManagedMemoryMB { get; set; }
    public List<TtTableStats> Tables { get; set; } = new();

    public int    MainGeneration  => TtAggregate.Generation(Tables, "Main");
    public int    NullGeneration  => TtAggregate.Generation(Tables, "Null");
    public double MainHitPct      => TtAggregate.HitPct(Tables, "Main");
    public double MainLoadPct     => TtAggregate.LoadPct(Tables, "Main");
    public long   MainReplacements => TtAggregate.Sum(Tables, "Main", t => t.Replacements);
    public double NullHitPct      => TtAggregate.HitPct(Tables, "Null");
    public double NullLoadPct     => TtAggregate.LoadPct(Tables, "Null");
    public long   NullReplacements => TtAggregate.Sum(Tables, "Null", t => t.Replacements);

    public string Report { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
}
