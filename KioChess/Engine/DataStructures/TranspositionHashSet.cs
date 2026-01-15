using Engine.Models.Transposition;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Engine.DataStructures;

[SkipLocalsInit]
public class TranspositionHashSet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BucketEntry // 12 bytes
    {
        public TranspositionEntry Entry; // 6 bytes
        public uint Key; // 4 bytes
        public ushort Generation; // 2 bytes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Count() => Entry.Depth != 0 ? 1 : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly int GetPriority(ushort currentGeneration) =>
            Entry.Depth * _depthFactor - currentGeneration + Generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal readonly int GetPriority(ushort currentGeneration) =>
        //    Entry.Depth * _depthFactor + (Entry.Type == TranspositionEntryType.Exact ? _typeFactor : 0) - currentGeneration + Generation;

        public override readonly string ToString() => $"K:{Key}, G:{Generation}, E:[{Entry}]";
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
    private struct Bucket // 64 bytes - perfectly aligned to cache line
    {
        public BucketEntry Entry1;  // 12 bytes
        public BucketEntry Entry2;  // 12 bytes
        public BucketEntry Entry3;  // 12 bytes
        public BucketEntry Entry4;  // 12 bytes
        public BucketEntry Entry5;  // 12 bytes
        // 4 bytes padding automatically added by Size = 64
        // Total: 60 + 4 = 64 bytes
        // +25% capacity compared to 4-entry bucket!

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Count() => Entry1.Count() + Entry2.Count() + Entry3.Count() + Entry4.Count() + Entry5.Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Set(TranspositionEntry item, uint entryKey, ushort currentGeneration)
        {
            // All slots full - find the worst entry to replace
            ref BucketEntry worst = ref Entry1;
            int worstPriority = Entry1.GetPriority(currentGeneration);

            int priority = Entry2.GetPriority(currentGeneration);
            if (priority < worstPriority)
            {
                worst = ref Entry2;
                worstPriority = priority;
            }

            priority = Entry3.GetPriority(currentGeneration);
            if (priority < worstPriority)
            {
                worst = ref Entry3;
                worstPriority = priority;
            }

            priority = Entry4.GetPriority(currentGeneration);
            if (priority < worstPriority)
            {
                worst = ref Entry4;
                worstPriority = priority;
            }

            if (Entry5.GetPriority(currentGeneration) < worstPriority)
            {
                worst = ref Entry5;
            }

            // Replace the worst entry
            worst.Entry = item;
            worst.Key = entryKey;
            worst.Generation = currentGeneration;
        }
    }

    private ushort _currentGeneration;
    private readonly ulong _mask;
    private readonly Bucket[] _buckets;
    private const int KeyShift = 32;
    private const byte EmptySlotKey = 0;
    private static int _depthFactor;
    private static int _typeFactor;

    public TranspositionHashSet(int capacityMB, int depthFactor, int typeFactor)
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
        _currentGeneration = 0;
        _depthFactor = depthFactor;
        _typeFactor = typeFactor;
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
    public void NewGeneration() => _currentGeneration++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool TryGetValue(ulong key, out TranspositionEntry item)
    {
        ulong index = key & _mask;
        ref Bucket bucket = ref _buckets[index];
        var entryKey = (uint)(key >> KeyShift);

        // Check all 5 entries
        if (bucket.Entry1.Key == entryKey)
        {
            item = bucket.Entry1.Entry;
            return true;
        }

        if (bucket.Entry2.Key == entryKey)
        {
            item = bucket.Entry2.Entry;
            return true;
        }

        if (bucket.Entry3.Key == entryKey)
        {
            item = bucket.Entry3.Entry;
            return true;
        }

        if (bucket.Entry4.Key == entryKey)
        {
            item = bucket.Entry4.Entry;
            return true;
        }

        if (bucket.Entry5.Key == entryKey)
        {
            item = bucket.Entry5.Entry;
            return true;
        }

        item = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(ulong key, TranspositionEntry item)
    {
        ref Bucket bucket = ref _buckets[key & _mask];
        var entryKey = (uint)(key >> KeyShift);

        // Check Entry1
        if (bucket.Entry1.Key == entryKey)
        {
            bucket.Entry1.Entry = item;
            bucket.Entry1.Generation = _currentGeneration;
            return;
        }
        if (bucket.Entry1.Entry.Depth == EmptySlotKey)
        {
            bucket.Entry1.Entry = item;
            bucket.Entry1.Key = entryKey;
            bucket.Entry1.Generation = _currentGeneration;
            return;
        }

        // Check Entry2
        if (bucket.Entry2.Key == entryKey)
        {
            bucket.Entry2.Entry = item;
            bucket.Entry2.Generation = _currentGeneration;
            return;
        }
        if (bucket.Entry2.Entry.Depth == EmptySlotKey)
        {
            bucket.Entry2.Entry = item;
            bucket.Entry2.Key = entryKey;
            bucket.Entry2.Generation = _currentGeneration;
            return;
        }

        // Check Entry3
        if (bucket.Entry3.Key == entryKey)
        {
            bucket.Entry3.Entry = item;
            bucket.Entry3.Generation = _currentGeneration;
            return;
        }
        if (bucket.Entry3.Entry.Depth == EmptySlotKey)
        {
            bucket.Entry3.Entry = item;
            bucket.Entry3.Key = entryKey;
            bucket.Entry3.Generation = _currentGeneration;
            return;
        }

        // Check Entry4
        if (bucket.Entry4.Key == entryKey)
        {
            bucket.Entry4.Entry = item;
            bucket.Entry4.Generation = _currentGeneration;
            return;
        }
        if (bucket.Entry4.Entry.Depth == EmptySlotKey)
        {
            bucket.Entry4.Entry = item;
            bucket.Entry4.Key = entryKey;
            bucket.Entry4.Generation = _currentGeneration;
            return;
        }

        // Check Entry5
        if (bucket.Entry5.Key == entryKey)
        {
            bucket.Entry5.Entry = item;
            bucket.Entry5.Generation = _currentGeneration;
            return;
        }
        if (bucket.Entry5.Entry.Depth == EmptySlotKey)
        {
            bucket.Entry5.Entry = item;
            bucket.Entry5.Key = entryKey;
            bucket.Entry5.Generation = _currentGeneration;
            return;
        }

        // All 5 slots full - use replacement strategy
        bucket.Set(item, entryKey, _currentGeneration);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Array.Clear(_buckets);
}