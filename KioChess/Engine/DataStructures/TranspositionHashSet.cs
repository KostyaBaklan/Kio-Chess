using Engine.Models.Transposition;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures;

public class TranspositionHashSet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BucketEntry // 12 bytes
    {
        public TranspositionEntry Entry; // 6 bytes
        public uint Key; // 4 bytes
        public ushort Generation; // 2 bytes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count()
        {
            return Entry.Depth != 0 ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetPriority(ushort currentGeneration)
        {
            return Entry.Depth * 8 - currentGeneration + Generation;
        }

        public override string ToString()
        {
            return $"K:{Key}, G:{Generation}, E:[{Entry}]";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Bucket // 12*4 = 48 bytes
    {
        public BucketEntry Entry1;
        public BucketEntry Entry2;
        public BucketEntry Entry3;
        public BucketEntry Entry4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count()
        {
            return Entry1.Count() + Entry2.Count() + Entry3.Count() + Entry4.Count();
        }

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

            if (Entry4.GetPriority(currentGeneration) < worstPriority)
            {
                worst = ref Entry4;
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

    public TranspositionHashSet(int capacityMB)
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
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            int count = 0;
            foreach (var bucket in _buckets)
            {
                count += bucket.Count();
            }
            return count;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NewGeneration()
    {
        _currentGeneration++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(ulong key, out TranspositionEntry item)
    {
        ref Bucket bucket = ref _buckets[key & _mask];
        var entryKey = (uint)(key >> KeyShift);

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

        // All slots full - use replacement strategy
        bucket.Set(item, entryKey, _currentGeneration);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_buckets);
    }
}
