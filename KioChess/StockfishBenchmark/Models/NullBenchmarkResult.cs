using System.Text;

namespace StockfishBenchmark.Models;

public class NullMoveRecord
{
    public int    MoveNumber   { get; set; }
    public bool   IsEngineMove { get; set; }
    public short  MoveKey      { get; set; }
    public string UciMove      { get; set; }
    public string MoveNotation { get; set; }
    public int    Value        { get; set; }
    public double DurationMs   { get; set; }

    public long   NullAttempts  { get; set; }
    public long   NullFailHighs { get; set; }
    public long   NullNodes     { get; set; }
    public double NullMs        { get; set; }
    public long   DrawHits      { get; set; }
    public long   RootNoMoves   { get; set; }
    public long[] DepthAttempts  { get; set; }
    public long[] DepthFailHighs { get; set; }

    // Filled only in compare (forced-path replay) mode
    public bool   HasBaseline         { get; set; }
    public short  BaselineMoveKey     { get; set; }
    public string BaselineMove        { get; set; }
    public int    BaselineValue       { get; set; }
    public double BaselineDurationMs  { get; set; }
    public long   BaselineNullAttempts  { get; set; }
    public long   BaselineNullFailHighs { get; set; }
    public long   BaselineNullNodes     { get; set; }
    public double BaselineNullMs        { get; set; }

    public bool SameMove   => !HasBaseline || BaselineMoveKey == MoveKey;
    public int  ValueDelta => HasBaseline ? Value - BaselineValue : 0;
    public double SuccessRate => NullAttempts == 0 ? 0 : 100.0 * NullFailHighs / NullAttempts;
}

public class NullBenchmarkSummary
{
    public int    EngineMoves   { get; set; }
    public double TotalMs       { get; set; }
    public long   Attempts      { get; set; }
    public long   FailHighs     { get; set; }
    public long   Nodes         { get; set; }
    public double NullMs        { get; set; }
    public long   DrawHits      { get; set; }
    public long   RootNoMoves   { get; set; }
    public long[] DepthAttempts  { get; set; } = new long[32];
    public long[] DepthFailHighs { get; set; } = new long[32];

    // Compare mode
    public bool   IsCompare     { get; set; }
    public int    SameMoves     { get; set; }
    public double AvgAbsValueDelta { get; set; }
    public double BaselineTotalMs  { get; set; }
    public long   BaselineAttempts  { get; set; }
    public long   BaselineFailHighs { get; set; }
    public double BaselineNullMs    { get; set; }

    public double SuccessRate => Attempts == 0 ? 0 : 100.0 * FailHighs / Attempts;
    public double NullTimeShare => TotalMs <= 0 ? 0 : 100.0 * NullMs / TotalMs;

    public string ToText()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Engine moves: {EngineMoves}   Total time: {TotalMs:F0} ms");
        sb.AppendLine($"Null attempts: {Attempts}   fail-high: {FailHighs} ({SuccessRate:F1}%)");
        sb.AppendLine($"Null nodes: {Nodes}   Null time: {NullMs:F0} ms ({NullTimeShare:F1}% of search)");
        sb.AppendLine($"Draw hits in null subtree: {DrawHits}   Null root without moves: {RootNoMoves}");

        if (IsCompare)
        {
            double bShare = BaselineTotalMs <= 0 ? 0 : 100.0 * BaselineNullMs / BaselineTotalMs;
            double bRate  = BaselineAttempts == 0 ? 0 : 100.0 * BaselineFailHighs / BaselineAttempts;
            sb.AppendLine($"Baseline: time {BaselineTotalMs:F0} ms, null attempts {BaselineAttempts}, fail-high {bRate:F1}%, null time {BaselineNullMs:F0} ms ({bShare:F1}%)");
            sb.AppendLine($"Same best move: {SameMoves}/{EngineMoves}   Avg |value delta|: {AvgAbsValueDelta:F1}");
        }

        sb.Append("By depth (attempts/fail-high): ");
        for (int d = 0; d < DepthAttempts.Length; d++)
            if (DepthAttempts[d] > 0)
                sb.Append($"d{d}:{DepthAttempts[d]}/{DepthFailHighs[d]}  ");
        return sb.ToString();
    }

    public static NullBenchmarkSummary Compute(List<NullMoveRecord> moves, bool isCompare)
    {
        var s = new NullBenchmarkSummary { IsCompare = isCompare };
        var eng = moves.Where(m => m.IsEngineMove).ToList();
        s.EngineMoves = eng.Count;
        foreach (var m in eng)
        {
            s.TotalMs += m.DurationMs;
            s.Attempts += m.NullAttempts;
            s.FailHighs += m.NullFailHighs;
            s.Nodes += m.NullNodes;
            s.NullMs += m.NullMs;
            s.DrawHits += m.DrawHits;
            s.RootNoMoves += m.RootNoMoves;
            if (m.DepthAttempts != null)
                for (int i = 0; i < s.DepthAttempts.Length && i < m.DepthAttempts.Length; i++)
                {
                    s.DepthAttempts[i] += m.DepthAttempts[i];
                    s.DepthFailHighs[i] += m.DepthFailHighs[i];
                }
            if (isCompare)
            {
                s.BaselineTotalMs += m.BaselineDurationMs;
                s.BaselineAttempts += m.BaselineNullAttempts;
                s.BaselineFailHighs += m.BaselineNullFailHighs;
                s.BaselineNullMs += m.BaselineNullMs;
                if (m.SameMove) s.SameMoves++;
            }
        }
        if (isCompare && eng.Count > 0)
            s.AvgAbsValueDelta = eng.Average(m => Math.Abs((double)m.ValueDelta));
        return s;
    }
}

public class NullBenchmarkResult
{
    public string Kind { get; set; } = "null";
    public bool   NullStatsEnabled { get; set; }
    public string EndReason { get; set; }
    public BenchmarkSession Session { get; set; }
    public List<NullMoveRecord> Moves { get; set; } = new();
    public NullBenchmarkSummary Summary { get; set; }
}
