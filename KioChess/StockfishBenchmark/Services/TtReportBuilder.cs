using Engine.DataStructures;
using StockfishBenchmark.Models;
using System.Text;

namespace StockfishBenchmark.Services;

public static class TtReportBuilder
{
    private const int BucketBytes = 64;

    public static string BuildReport(TtVariantResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== {r.Name} ===");
        sb.AppendLine($"Engine moves: {r.EngineMoves}, same move as baseline: {r.MatchingMoves}, search time: {r.TotalMs:F0} ms (avg {r.AvgMs:F0} ms)");
        sb.AppendLine($"Nodes: {r.Nodes:N0} search + {r.QNodes:N0} quiescence = {r.Nodes + r.QNodes:N0} ({r.Knps:F0} knps, avg {(r.EngineMoves == 0 ? 0 : (r.Nodes + r.QNodes) / r.EngineMoves):N0} per move)");
        sb.Append(BuildTablesReport(r.Tables));
        return sb.ToString();
    }

    public static string BuildTablesReport(IEnumerable<TtTableStats> tables)
    {
        var sb = new StringBuilder();
        if (!TtStatistics.Enabled)
        {
            sb.AppendLine("Engine was built without TT_STATS: only timing is available. Set <TtStats>true</TtStats> in Engine.csproj.");
            return sb.ToString();
        }

        foreach (var t in tables)
        {
            sb.AppendLine();
            AppendTable(sb, t);
        }
        return sb.ToString();
    }

    private static void AppendTable(StringBuilder sb, TtTableStats t)
    {
        double mb = t.Buckets * (double)BucketBytes / (1024 * 1024);
        sb.AppendLine($"--- {t.Name}: {mb:F0} MB, {t.Buckets:N0} buckets, {t.Capacity:N0} entries, generation {t.Generation}, depthFactor {t.DepthFactor} ---");
        sb.AppendLine($"Occupied: {t.Occupied:N0} ({t.Load:P1})");

        sb.AppendLine();
        sb.AppendLine("Probes");
        sb.AppendLine($"  probes {t.Probes:N0}, hits {t.Hits:N0} ({t.HitRate:P1}), empty-slot key matches {t.EmptySlotKeyMatches:N0}");
        sb.AppendLine($"  hits by slot   : {Row(t.HitsBySlot, t.Hits)}");
        sb.AppendLine($"  hit entry age  : {AgeRow(t.HitAge, t.Hits)}");

        sb.AppendLine();
        sb.AppendLine("Stores");
        sb.AppendLine($"  stores {t.Stores:N0}: updates {t.Updates:N0} ({Pct(t.Updates, t.Stores)}), fills {t.Fills:N0} ({Pct(t.Fills, t.Stores)}), replacements {t.Replacements:N0} ({Pct(t.Replacements, t.Stores)})");
        sb.AppendLine($"  updates that lowered depth: {t.UpdateDowngrades:N0} ({Pct(t.UpdateDowngrades, t.Updates)} of updates)");
        sb.AppendLine($"  updates by slot: {Row(t.UpdatesBySlot, t.Updates)}");
        sb.AppendLine($"  fills by slot  : {Row(t.FillsBySlot, t.Fills)}");
        sb.AppendLine($"  update old age : {AgeRow(t.UpdateAge, t.Updates)}");

        sb.AppendLine();
        sb.AppendLine("Replacements");
        if (t.Replacements == 0)
        {
            sb.AppendLine("  none: the table never had to evict an entry");
        }
        else
        {
            sb.AppendLine($"  victims by slot     : {Row(t.ReplacementsBySlot, t.Replacements)}");
            sb.AppendLine($"  victim age          : {AgeRow(t.VictimAge, t.Replacements)}");
            sb.AppendLine($"  avg depth victim/new: {(double)t.VictimDepthSum / t.Replacements:F2} / {(double)t.NewDepthSum / t.Replacements:F2}");
            sb.AppendLine($"  victim from current generation: {t.VictimCurrentGeneration:N0} ({Pct(t.VictimCurrentGeneration, t.Replacements)})");
            sb.AppendLine($"  victim deeper than new entry   : {t.VictimDeeperThanNew:N0} ({Pct(t.VictimDeeperThanNew, t.Replacements)})");
            sb.AppendLine($"  victim not the shallowest (generation changed the choice): {t.VictimNotShallowest:N0} ({Pct(t.VictimNotShallowest, t.Replacements)})");
            sb.AppendLine($"  depth tie broken by generation/slot order: {t.VictimTieBrokenByGeneration:N0} ({Pct(t.VictimTieBrokenByGeneration, t.Replacements)})");
            sb.AppendLine($"  FIRST replacement: store #{t.FirstReplacementStore:N0}, generation {t.FirstReplacementGeneration}, victim age {t.FirstReplacementVictimAge}, occupied {t.FirstReplacementOccupied:N0} ({(t.Capacity == 0 ? 0 : (double)t.FirstReplacementOccupied / t.Capacity):P1})");
            if (t.FirstOlderVictimStore > 0)
                sb.AppendLine($"  FIRST replacement of an older-generation entry: store #{t.FirstOlderVictimStore:N0}, generation {t.FirstOlderVictimGeneration}, age {t.FirstOlderVictimAge}");
            else
                sb.AppendLine("  no older-generation entry was ever evicted");
        }

        sb.AppendLine();
        sb.AppendLine("Entry types (E=exact, L=lower, U=upper)");
        sb.AppendLine($"  stores E/L/U   : {TypeRow(t.StoresByType)}");
        sb.AppendLine($"  hits E/L/U     : {TypeRow(t.HitsByType)}");
        sb.AppendLine($"  resident E/L/U : {TypeRow(t.ResidentByType)}");
        sb.AppendLine($"  updates (old -> new)      : {Transitions(t.UpdateTransitions)}");
        sb.AppendLine($"  replacements (victim -> new): {Transitions(t.ReplacementTransitions)}");

        sb.AppendLine();
        sb.AppendLine("Per generation");
        sb.AppendLine("  gen    probes     hit%      stores       fills   replaced   fromOlder   occupied%");
        foreach (var g in t.Generations)
        {
            sb.AppendLine($"  {g.Generation,3} {g.Probes,9:N0} {Pct(g.Hits, g.Probes),8} {g.Stores,11:N0} {g.Fills,11:N0} {g.Replacements,10:N0} {g.VictimsFromOlderGeneration,11:N0} {(t.Capacity == 0 ? 0 : 100.0 * g.OccupiedAtEnd / t.Capacity),10:F1}");
        }

        sb.AppendLine();
        sb.AppendLine("Final table scan");
        sb.AppendLine($"  bucket fill (0..5 entries): {Row(t.BucketFill, t.Buckets)}");
        sb.AppendLine($"  entry age                 : {AgeRow(t.AgeHistogram, t.Occupied)}");
        sb.AppendLine("  slot  occupied%  avgDepth  priority min/avg/max   age0%  age1%  age2-3%  age4+%");
        for (int i = 0; i < TtTableStats.Slots; i++)
        {
            long n = t.SlotOccupied[i];
            if (n == 0)
            {
                sb.AppendLine($"  {i + 1,4}  {0.0,9:F1}  empty");
                continue;
            }
            long a0 = t.SlotAge[i][0], a1 = t.SlotAge[i][1];
            long a23 = t.SlotAge[i][2] + t.SlotAge[i][3];
            long a4 = n - a0 - a1 - a23;
            sb.AppendLine($"  {i + 1,4}  {100.0 * n / t.Buckets,9:F1}  {(double)t.SlotDepthSum[i] / n,8:F2}  {t.SlotPriorityMin[i]}/{(double)t.SlotPrioritySum[i] / n:F0}/{t.SlotPriorityMax[i]}  {Pct(a0, n),6} {Pct(a1, n),6} {Pct(a23, n),8} {Pct(a4, n),7}");
        }
        sb.AppendLine($"  depth histogram: {DepthRow(t.DepthHistogram, t.Occupied)}");
    }

    private static string TypeRow(long[] v)
    {
        long total = v.Sum();
        return $"{v[0]:N0} ({Pct(v[0], total)}) / {v[1]:N0} ({Pct(v[1], total)}) / {v[2]:N0} ({Pct(v[2], total)})";
    }

    private static string Transitions(long[] m)
    {
        string[] n = { "E", "L", "U" };
        var parts = new List<string>();
        for (int i = 0; i < TtTableStats.Types; i++)
            for (int j = 0; j < TtTableStats.Types; j++)
                parts.Add($"{n[i]}>{n[j]} {m[i * TtTableStats.Types + j]:N0}");
        return string.Join(", ", parts);
    }

    public static string BuildAdvice(TtVariantResult r) => BuildAdvice(r.Name, r.Tables);

    public static string BuildAdvice(string name, IEnumerable<TtTableStats> tables)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== Advice: {name} ===");

        if (!TtStatistics.Enabled)
        {
            sb.AppendLine("No statistics (build Engine with TtStats=true).");
            return sb.ToString();
        }

        foreach (var t in tables)
        {
            sb.AppendLine($"[{t.Name}]");
            if (t.Stores == 0) { sb.AppendLine("  no stores"); continue; }

            double mb = t.Buckets * (double)BucketBytes / (1024 * 1024);
            long hitsOlder = t.Hits - t.HitAge[0];
            double olderShare = t.Hits == 0 ? 0 : (double)hitsOlder / t.Hits;
            double genInfluence = t.Replacements == 0 ? 0 : (double)(t.VictimNotShallowest + t.VictimTieBrokenByGeneration) / t.Replacements;

            if (t.Generation == 0)
                sb.AppendLine("  generation was never advanced for this table (all entries are generation 0).");

            if (t.Replacements == 0)
                sb.AppendLine($"  never full: load {t.Load:P1} of {mb:F0} MB. Size can be reduced (about {Math.Max(1, Math.Ceiling(mb * Math.Max(t.Load, 0.05) * 2)):F0} MB keeps 2x headroom for this game).");
            else if (t.Load < 0.5 && t.Replacements * 20 < t.Stores)
                sb.AppendLine($"  replacements are rare ({Pct(t.Replacements, t.Stores)} of stores) and load is {t.Load:P1}: table is probably oversized.");

            if (t.Replacements > 0 && t.VictimCurrentGeneration * 2 > t.Replacements && t.Replacements * 5 > t.Stores)
                sb.AppendLine($"  under-sized: {Pct(t.VictimCurrentGeneration, t.Replacements)} of victims belong to the current generation and {Pct(t.Replacements, t.Stores)} of stores evict something. Try a larger table.");

            if (t.Generation > 0)
            {
                sb.AppendLine($"  hits on entries from older generations: {olderShare:P1}.");
                if (olderShare < 0.02)
                    sb.AppendLine("  older-generation entries are almost never reused: generation adds little benefit in this run.");
                sb.AppendLine($"  generation changed the eviction choice in {genInfluence:P1} of replacements.");
                if (t.Replacements > 0 && genInfluence < 0.05 && olderShare < 0.05)
                    sb.AppendLine("  candidate: drop the 2-byte generation. Entry becomes 10 bytes, 6 entries fit into the 64-byte bucket (+20% capacity at equal memory). Validate with a match test.");
            }

            if (t.Updates > 0 && (double)t.UpdateDowngrades / t.Updates > 0.1)
                sb.AppendLine($"  {Pct(t.UpdateDowngrades, t.Updates)} of same-key updates replaced a deeper entry with a shallower one. Consider keeping the deeper entry.");

            if (t.HitsBySlot.Sum() > 0)
            {
                double firstSlot = (double)t.HitsBySlot[0] / t.HitsBySlot.Sum();
                sb.AppendLine($"  {firstSlot:P1} of hits are found in slot 1 (probe order is fixed 1..5).");
            }

            double full = t.Buckets == 0 ? 0 : (double)t.BucketFill[TtTableStats.Slots] / t.Buckets;
            sb.AppendLine($"  buckets completely full at the end: {full:P1}.");
        }
        return sb.ToString();
    }

    private static string Pct(long part, long total) =>
        total == 0 ? "-" : $"{100.0 * part / total:F1}%";

    private static string Row(long[] values, long total)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append("  ");
            sb.Append($"[{i}] {values[i]:N0} ({Pct(values[i], total)})");
        }
        return sb.ToString();
    }

    private static string AgeRow(long[] ages, long total)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < ages.Length; i++)
        {
            if (ages[i] == 0) continue;
            if (sb.Length > 0) sb.Append("  ");
            sb.Append($"{(i == ages.Length - 1 ? i + "+" : i.ToString())}: {Pct(ages[i], total)}");
        }
        return sb.Length == 0 ? "-" : sb.ToString();
    }

    private static string DepthRow(long[] depths, long total)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < depths.Length; i++)
        {
            if (depths[i] == 0) continue;
            if (sb.Length > 0) sb.Append("  ");
            sb.Append($"d{i}: {Pct(depths[i], total)}");
        }
        return sb.Length == 0 ? "-" : sb.ToString();
    }
}
