namespace NnueTool;

internal sealed class BloomFilter
{
    private readonly ulong[] _bits;
    private readonly ulong _mask;

    public BloomFilter(int bitsPower)
    {
        _bits = new ulong[(1L << bitsPower) >> 6];
        _mask = (1UL << bitsPower) - 1;
    }

    /// <summary>Returns true if the key was not present (probabilistically).</summary>
    public bool TryAdd(ulong key)
    {
        ulong h1 = key * 0x9E3779B97F4A7C15UL;
        ulong h2 = (key ^ (key >> 31)) * 0xBF58476D1CE4E5B9UL | 1;

        bool added = false;
        for (int i = 0; i < 3; i++)
        {
            ulong bit = (h1 + (ulong)i * h2) & _mask;
            ref ulong word = ref _bits[bit >> 6];
            ulong flag = 1UL << (int)(bit & 63);
            if ((word & flag) == 0)
            {
                word |= flag;
                added = true;
            }
        }
        return added;
    }
}
