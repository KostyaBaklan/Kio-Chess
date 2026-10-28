namespace Engine.DataStructures;

/// <summary>
/// Per-generation counters of a transposition hash set (statistics build only, see TT_STATS).
/// </summary>
public sealed class TtGenerationStats
{
    public int Generation;
    public long Probes;
    public long Hits;
    public long Stores;
    public long Updates;
    public long Fills;
    public long Replacements;
    public long VictimsFromOlderGeneration;
    public long OccupiedAtEnd;

    public TtGenerationStats Clone() => (TtGenerationStats)MemberwiseClone();
}

/// <summary>
/// Counters and end-of-run table scan of one transposition hash set (statistics build only, see TT_STATS).
/// Index conventions: slot = position inside the bucket (0..4), age = current generation - entry generation
/// (last bin = "this or more"), depth bins are clamped to the last bin.
/// </summary>
public sealed class TtTableStats
{
    public const int Slots = 5;
    public const int AgeBins = 16;
    public const int DepthBins = 32;
    public const int Types = 3; // Exact, LowerBound, UpperBound

    // Entry type counters. Transition arrays are indexed old * Types + new (victim * Types + new for replacements).
    public long[] HitsByType = new long[Types];
    public long[] StoresByType = new long[Types];
    public long[] UpdateTransitions = new long[Types * Types];
    public long[] ReplacementTransitions = new long[Types * Types];
    public long[] ResidentByType = new long[Types];

    public string Name;
    public long Buckets;
    public int Generation;
    public int DepthFactor;
    public long Occupied;

    // Probes
    public long Probes;
    public long Hits;
    public long EmptySlotKeyMatches;
    public long[] HitsBySlot = new long[Slots];
    public long[] HitAge = new long[AgeBins];

    // Stores
    public long Stores;
    public long Updates;
    public long UpdateDowngrades;
    public long Fills;
    public long Replacements;
    public long[] UpdatesBySlot = new long[Slots];
    public long[] UpdateAge = new long[AgeBins];
    public long[] FillsBySlot = new long[Slots];
    public long[] ReplacementsBySlot = new long[Slots];

    // Replacement quality
    public long[] VictimAge = new long[AgeBins];
    public long[] VictimDepth = new long[DepthBins];
    public long VictimDepthSum;
    public long NewDepthSum;
    public long VictimCurrentGeneration;
    public long VictimDeeperThanNew;
    public long VictimNotShallowest;
    public long VictimTieBrokenByGeneration;

    // First replacement
    public long FirstReplacementStore;
    public int FirstReplacementGeneration;
    public int FirstReplacementVictimAge;
    public long FirstReplacementOccupied;
    public long FirstOlderVictimStore;
    public int FirstOlderVictimGeneration;
    public int FirstOlderVictimAge;

    // End-of-run scan
    public long[] BucketFill = new long[Slots + 1];
    public long[] SlotOccupied = new long[Slots];
    public long[] SlotDepthSum = new long[Slots];
    public long[] SlotPrioritySum = new long[Slots];
    public int[] SlotPriorityMin = new int[Slots];
    public int[] SlotPriorityMax = new int[Slots];
    public long[][] SlotAge = CreateSlotAge();
    public long[] AgeHistogram = new long[AgeBins];
    public long[] DepthHistogram = new long[DepthBins];

    public List<TtGenerationStats> Generations = new();

    public long Capacity => Buckets * Slots;
    public double HitRate => Probes == 0 ? 0 : (double)Hits / Probes;
    public double Load => Capacity == 0 ? 0 : (double)Occupied / Capacity;

    private static long[][] CreateSlotAge()
    {
        var result = new long[Slots][];
        for (int i = 0; i < Slots; i++) result[i] = new long[AgeBins];
        return result;
    }

    public TtTableStats Clone()
    {
        var c = (TtTableStats)MemberwiseClone();
        c.HitsBySlot = (long[])HitsBySlot.Clone();
        c.HitAge = (long[])HitAge.Clone();
        c.UpdatesBySlot = (long[])UpdatesBySlot.Clone();
        c.UpdateAge = (long[])UpdateAge.Clone();
        c.FillsBySlot = (long[])FillsBySlot.Clone();
        c.ReplacementsBySlot = (long[])ReplacementsBySlot.Clone();
        c.VictimAge = (long[])VictimAge.Clone();
        c.VictimDepth = (long[])VictimDepth.Clone();
        c.BucketFill = (long[])BucketFill.Clone();
        c.SlotOccupied = (long[])SlotOccupied.Clone();
        c.SlotDepthSum = (long[])SlotDepthSum.Clone();
        c.SlotPrioritySum = (long[])SlotPrioritySum.Clone();
        c.SlotPriorityMin = (int[])SlotPriorityMin.Clone();
        c.SlotPriorityMax = (int[])SlotPriorityMax.Clone();
        c.SlotAge = new long[Slots][];
        for (int i = 0; i < Slots; i++) c.SlotAge[i] = (long[])SlotAge[i].Clone();
        c.AgeHistogram = (long[])AgeHistogram.Clone();
        c.HitsByType = (long[])HitsByType.Clone();
        c.StoresByType = (long[])StoresByType.Clone();
        c.UpdateTransitions = (long[])UpdateTransitions.Clone();
        c.ReplacementTransitions = (long[])ReplacementTransitions.Clone();
        c.ResidentByType = (long[])ResidentByType.Clone();
        c.DepthHistogram = (long[])DepthHistogram.Clone();
        c.Generations = new List<TtGenerationStats>(Generations.Count);
        foreach (var g in Generations) c.Generations.Add(g.Clone());
        return c;
    }
}
