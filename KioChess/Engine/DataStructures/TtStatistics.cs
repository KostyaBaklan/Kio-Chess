namespace Engine.DataStructures;

/// <summary>
/// Registry of transposition hash sets that collect statistics.
/// Counters are compiled in only when the Engine project is built with the TT_STATS symbol
/// (set TtStats to true in Engine.csproj). Otherwise <see cref="Enabled"/> is false and every call is a no-op.
/// </summary>
public static class TtStatistics
{
    public const bool Enabled =
#if TT_STATS
        true;
#else
        false;
#endif

    /// <summary>Search calls (SearchWhite/SearchBlack) counted while TT_STATS is defined.</summary>
    public static long Nodes;

    /// <summary>Quiescence entries (EvaluateWhite/EvaluateBlack) counted while TT_STATS is defined.</summary>
    public static long QNodes;

#if TT_STATS
    private static readonly List<WeakReference<TranspositionHashSet>> Sets = new();
    private static readonly object Sync = new();

    internal static void Register(TranspositionHashSet set)
    {
        lock (Sync)
        {
            Sets.RemoveAll(w => !w.TryGetTarget(out _));
            Sets.Add(new WeakReference<TranspositionHashSet>(set));
        }
    }
#endif

    /// <summary>Zeroes the counters of every live table (table content is untouched).</summary>
    public static void Reset()
    {
        Nodes = 0;
        QNodes = 0;
#if TT_STATS
        lock (Sync)
        {
            foreach (var w in Sets)
            {
                if (w.TryGetTarget(out var set)) set.ResetStats();
            }
        }
#endif
    }

    /// <summary>Captures counters plus an occupancy scan of every live table that saw any activity.</summary>
    public static List<TtTableStats> Capture()
    {
        var result = new List<TtTableStats>();
#if TT_STATS
        lock (Sync)
        {
            foreach (var w in Sets)
            {
                if (!w.TryGetTarget(out var set)) continue;
                var stats = set.CaptureStats();
                if (stats.Probes + stats.Stores > 0) result.Add(stats);
            }
        }
        result.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
#endif
        return result;
    }
}
