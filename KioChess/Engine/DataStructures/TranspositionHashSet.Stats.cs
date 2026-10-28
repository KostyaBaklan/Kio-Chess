#if TT_STATS
using Engine.Models.Transposition;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

public partial class TranspositionHashSet
{
    private TtTableStats _stats;
    private TtGenerationStats _gen;
    private List<TtGenerationStats> _generations;
    private long _occupied;

    public string Name { get; private set; }

    private void InitStats(string name)
    {
        Name = name ?? "TT";
        _occupied = 0;
        StartStats();
        TtStatistics.Register(this);
    }

    private void StartStats()
    {
        _stats = new TtTableStats { Name = Name, Buckets = _buckets.Length };
        _generations = new List<TtGenerationStats>();
        _gen = new TtGenerationStats { Generation = _currentGeneration };
        _generations.Add(_gen);
    }

    internal void ResetStats()
    {
        StartStats();
        _occupied = CountOccupied();
    }

    private long CountOccupied()
    {
        long count = 0;
        for (int b = 0; b < _buckets.Length; b++) count += _buckets[b].Count();
        return count;
    }

    private void OnNewGenerationStats()
    {
        _gen.OccupiedAtEnd = _occupied;
        _gen = new TtGenerationStats { Generation = _currentGeneration };
        _generations.Add(_gen);
    }

    private void OnClearStats() => _occupied = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int AgeBin(ushort entryGeneration) =>
        Math.Min((int)(ushort)(_currentGeneration - entryGeneration), TtTableStats.AgeBins - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DepthBin(int depth) =>
        depth < 0 ? 0 : Math.Min(depth, TtTableStats.DepthBins - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int TypeBin(TranspositionEntryType type) =>
        (int)type < TtTableStats.Types ? (int)type : TtTableStats.Types - 1;

    private TranspositionEntry GetValueInstrumented(ulong key)
    {
        ref Bucket bucket = ref _buckets[key & _mask];
        var entryKey = (uint)(key >> KeyShift);
        var s = _stats;
        s.Probes++;
        _gen.Probes++;

        for (int i = 0; i < TtTableStats.Slots; i++)
        {
            ref BucketEntry e = ref Unsafe.Add(ref bucket.Entry1, i);
            if (e.Key != entryKey) continue;

            if (e.Entry.Depth == EmptySlotKey)
            {
                s.EmptySlotKeyMatches++;
                return e.Entry;
            }

            s.Hits++;
            _gen.Hits++;
            s.HitsBySlot[i]++;
            s.HitAge[AgeBin(e.Generation)]++;
            s.HitsByType[TypeBin(e.Entry.Type)]++;
            return e.Entry;
        }
        return _default;
    }

    private void SetInstrumented(ulong key, TranspositionEntry item)
    {
        ref Bucket bucket = ref _buckets[key & _mask];
        var entryKey = (uint)(key >> KeyShift);
        var s = _stats;
        var g = _gen;
        s.Stores++;
        g.Stores++;
        s.StoresByType[TypeBin(item.Type)]++;
        int newOccupied = item.Depth != 0 ? 1 : 0;

        for (int i = 0; i < TtTableStats.Slots; i++)
        {
            ref BucketEntry e = ref Unsafe.Add(ref bucket.Entry1, i);

            if (e.Key == entryKey)
            {
                s.Updates++;
                g.Updates++;
                s.UpdatesBySlot[i]++;
                s.UpdateAge[AgeBin(e.Generation)]++;
                if (item.Depth < e.Entry.Depth) s.UpdateDowngrades++;
                s.UpdateTransitions[TypeBin(e.Entry.Type) * TtTableStats.Types + TypeBin(item.Type)]++;
                _occupied += newOccupied - (e.Entry.Depth != 0 ? 1 : 0);
                e.Entry = item;
                return;
            }

            if (e.Entry.Depth == EmptySlotKey)
            {
                s.Fills++;
                g.Fills++;
                s.FillsBySlot[i]++;
                _occupied += newOccupied;
                e.Entry = item;
                e.Key = entryKey;
                return;
            }
        }

        // Same victim selection as Bucket.Set, plus the depth-only alternative for comparison.
        int victim = 0;
        int victimPriority = bucket.Entry1.GetPriority();
        int shallowest = 0;
        int shallowestDepth = bucket.Entry1.Entry.Depth;
        for (int i = 1; i < TtTableStats.Slots; i++)
        {
            ref BucketEntry e = ref Unsafe.Add(ref bucket.Entry1, i);
            int priority = e.GetPriority();
            if (priority < victimPriority) { victim = i; victimPriority = priority; }
            if (e.Entry.Depth < shallowestDepth) { shallowest = i; shallowestDepth = e.Entry.Depth; }
        }

        ref BucketEntry v = ref Unsafe.Add(ref bucket.Entry1, victim);
        int age = AgeBin(v.Generation);
        int victimDepth = v.Entry.Depth;

        s.Replacements++;
        g.Replacements++;
        s.ReplacementsBySlot[victim]++;
        s.VictimAge[age]++;
        s.VictimDepth[DepthBin(victimDepth)]++;
        s.VictimDepthSum += victimDepth;
        s.ReplacementTransitions[TypeBin(v.Entry.Type) * TtTableStats.Types + TypeBin(item.Type)]++;
        s.NewDepthSum += item.Depth;
        if (age == 0) s.VictimCurrentGeneration++; else g.VictimsFromOlderGeneration++;
        if (victimDepth > item.Depth) s.VictimDeeperThanNew++;
        if (victimDepth > shallowestDepth) s.VictimNotShallowest++;
        else if (victim != shallowest) s.VictimTieBrokenByGeneration++;

        if (s.Replacements == 1)
        {
            s.FirstReplacementStore = s.Stores;
            s.FirstReplacementGeneration = _currentGeneration;
            s.FirstReplacementVictimAge = age;
            s.FirstReplacementOccupied = _occupied;
        }
        if (age > 0 && s.FirstOlderVictimStore == 0)
        {
            s.FirstOlderVictimStore = s.Stores;
            s.FirstOlderVictimGeneration = _currentGeneration;
            s.FirstOlderVictimAge = age;
        }

        _occupied += newOccupied - 1;
        bucket.Set(item, entryKey);
    }

    internal TtTableStats CaptureStats()
    {
        var c = _stats.Clone();
        c.Generation = _currentGeneration;
        c.DepthFactor = _depthFactor;
        c.Occupied = _occupied;

        c.Generations = new List<TtGenerationStats>(_generations.Count);
        foreach (var g in _generations) c.Generations.Add(g.Clone());
        c.Generations[^1].OccupiedAtEnd = _occupied;

        for (int i = 0; i < TtTableStats.Slots; i++) c.SlotPriorityMin[i] = int.MaxValue;
        for (int i = 0; i < TtTableStats.Slots; i++) c.SlotPriorityMax[i] = int.MinValue;

        for (int b = 0; b < _buckets.Length; b++)
        {
            ref Bucket bucket = ref _buckets[b];
            int used = 0;
            for (int i = 0; i < TtTableStats.Slots; i++)
            {
                ref BucketEntry e = ref Unsafe.Add(ref bucket.Entry1, i);
                if (e.Entry.Depth == EmptySlotKey) continue;

                used++;
                int age = AgeBin(e.Generation);
                int priority = e.GetPriority();
                c.SlotOccupied[i]++;
                c.SlotDepthSum[i] += e.Entry.Depth;
                c.SlotPrioritySum[i] += priority;
                if (priority < c.SlotPriorityMin[i]) c.SlotPriorityMin[i] = priority;
                if (priority > c.SlotPriorityMax[i]) c.SlotPriorityMax[i] = priority;
                c.SlotAge[i][age]++;
                c.AgeHistogram[age]++;
                c.ResidentByType[TypeBin(e.Entry.Type)]++;
                c.DepthHistogram[DepthBin(e.Entry.Depth)]++;
            }
            c.BucketFill[used]++;
        }
        return c;
    }
}
#endif
