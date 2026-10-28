using Engine.Models.Transposition;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures;

[SkipLocalsInit]
public partial class TranspositionHashSet
{
    // AOS layout — key and entry are adjacent so OoO execution can load the entry
    // speculatively while the key comparison resolves.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BucketEntry // 10 bytes
    {
        public TranspositionEntry Entry; // 6 bytes
        public uint Key;               // 4 bytes

        // Generations were removed; kept as a constant so the statistics code keeps working.
        public readonly ushort Generation => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Count() => Entry.Depth != 0 ? 1 : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly int GetPriority() => Entry.Depth * _depthFactor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public override readonly string ToString() => $"K:{Key}, E:[{Entry}]";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
    private struct Bucket // 64 bytes - perfectly aligned to cache line
    {
        public BucketEntry Entry1; // 10 bytes
        public BucketEntry Entry2; // 10 bytes
        public BucketEntry Entry3; // 10 bytes
        public BucketEntry Entry4; // 10 bytes
        public BucketEntry Entry5; // 10 bytes
        public BucketEntry Entry6; // 10 bytes
        // 4 bytes padding (Size = 64)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Count() =>
            Entry1.Count() + Entry2.Count() + Entry3.Count() +
            Entry4.Count() + Entry5.Count() + Entry6.Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Set(TranspositionEntry item, uint entryKey)
        {
            // Single-pass: find the worst-priority entry to evict
            ref BucketEntry worst = ref Entry1;
            int worstPriority = Entry1.GetPriority();

            int priority = Entry2.GetPriority();
            if (priority < worstPriority) { worst = ref Entry2; worstPriority = priority; }

            priority = Entry3.GetPriority();
            if (priority < worstPriority) { worst = ref Entry3; worstPriority = priority; }

            priority = Entry4.GetPriority();
            if (priority < worstPriority) { worst = ref Entry4; worstPriority = priority; }

            priority = Entry5.GetPriority();
            if (priority < worstPriority) { worst = ref Entry5; worstPriority = priority; }

            if (Entry6.GetPriority() < worstPriority) { worst = ref Entry6; }

            worst.Entry = item;
            worst.Key = entryKey;
        }
    }

    private readonly ulong _mask;
    private readonly Bucket[] _buckets;
    private const int KeyShift = 32;
    private const byte EmptySlotKey = 0;
    private static int _depthFactor;
    private static int _typeFactor;
    private readonly TranspositionEntry _default = new();

    public TranspositionHashSet(int capacityMB, int depthFactor, int typeFactor, string name = null)
    {
        var bucketSize = Unsafe.SizeOf<Bucket>();
        int maxBytes = capacityMB * 1024 * 1024;
        int bucketCount = maxBytes / bucketSize;

        // Round down to power of 2 to stay within memory budget
        if (bucketCount > 0)
        {
            int highestBit = BitOperations.Log2((uint)bucketCount);
            bucketCount = 1 << highestBit;
        }
        else
        {
            bucketCount = 1;
        }

        _buckets = new Bucket[bucketCount];
        _mask = (ulong)(bucketCount - 1);
        _depthFactor = depthFactor;
        _typeFactor = typeFactor;
#if TT_STATS
        InitStats(name);
#endif
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            int count = 0;
            for (int i = 0; i < _buckets.Length; i++)
            {
                count += _buckets[i].Count();
            }
            return count;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NewGeneration()
    {
        // Generations were removed from the replacement policy: nothing to advance.
#if TT_STATS
        OnNewGenerationStats();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionEntry GetValue(ulong key)
    {
#if TT_STATS
        return GetValueInstrumented(key);
#else
        return GetValueCore(key);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TranspositionEntry GetValueCore(ulong key)
    {
        ref Bucket bucket = ref _buckets[key & _mask];
        var entryKey = (uint)(key >> KeyShift);

        // Named-field
        // the Key comparison (OoO execution), so hits cost ~zero extra cycles.
        if (bucket.Entry1.Key == entryKey) return bucket.Entry1.Entry;
        if (bucket.Entry2.Key == entryKey) return bucket.Entry2.Entry;
        if (bucket.Entry3.Key == entryKey) return bucket.Entry3.Entry;
        if (bucket.Entry4.Key == entryKey) return bucket.Entry4.Entry;
        if (bucket.Entry5.Key == entryKey) return bucket.Entry5.Entry;
        if (bucket.Entry6.Key == entryKey) return bucket.Entry6.Entry;
        return _default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(ulong key, TranspositionEntry item)
    {
#if TT_STATS
        SetInstrumented(key, item);
#else
        SetCore(key, item);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetCore(ulong key, TranspositionEntry item)
    {
        ref Bucket bucket = ref _buckets[key & _mask];
        var entryKey = (uint)(key >> KeyShift);

        // Phase 1
        if (bucket.Entry1.Key == entryKey) { bucket.Entry1.Entry = item; return; }
        if (bucket.Entry1.Entry.Depth == EmptySlotKey) { bucket.Entry1.Entry = item; bucket.Entry1.Key = entryKey; return; }

        if (bucket.Entry2.Key == entryKey) { bucket.Entry2.Entry = item; return; }
        if (bucket.Entry2.Entry.Depth == EmptySlotKey) { bucket.Entry2.Entry = item; bucket.Entry2.Key = entryKey; return; }

        if (bucket.Entry3.Key == entryKey) { bucket.Entry3.Entry = item; return; }
        if (bucket.Entry3.Entry.Depth == EmptySlotKey) { bucket.Entry3.Entry = item; bucket.Entry3.Key = entryKey; return; }

        if (bucket.Entry4.Key == entryKey) { bucket.Entry4.Entry = item; return; }
        if (bucket.Entry4.Entry.Depth == EmptySlotKey) { bucket.Entry4.Entry = item; bucket.Entry4.Key = entryKey; return; }

        if (bucket.Entry5.Key == entryKey) { bucket.Entry5.Entry = item; return; }
        if (bucket.Entry5.Entry.Depth == EmptySlotKey) { bucket.Entry5.Entry = item; bucket.Entry5.Key = entryKey; return; }

        if (bucket.Entry6.Key == entryKey) { bucket.Entry6.Entry = item; return; }
        if (bucket.Entry6.Entry.Depth == EmptySlotKey) { bucket.Entry6.Entry = item; bucket.Entry6.Key = entryKey; return; }

        // Phase 3: all 6 slots occupied — evict lowest-priority entry
        bucket.Set(item, entryKey);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_buckets);
#if TT_STATS
        OnClearStats();
#endif
    }
}
