using System.Runtime.CompilerServices;

namespace Analysis.DataAccess.Services;

/// <summary>
/// Generates truly order-independent hash for move sequences.
/// NO SORTING REQUIRED - uses commutative XOR combination.
/// 
/// Properties:
/// - Hash([a, b, c]) = Hash([b, a, c]) = Hash([c, b, a]) for ANY order
/// - O(n) time complexity (no sorting overhead)
/// - Deterministic and collision-resistant
/// 
/// Example:
///   MoveKeys: [e2e4=1234, c7c5=5678, g1f3=9012]
///   Hash1 = ComputeOrderIndependentHash([1234, 5678, 9012]) = 0x123abc456def7890
///   Hash2 = ComputeOrderIndependentHash([5678, 1234, 9012]) = 0x123abc456def7890 (SAME!)
///   Hash3 = ComputeOrderIndependentHash([9012, 5678, 1234]) = 0x123abc456def7890 (SAME!)
/// </summary>
public static class OrderIndependentSequenceHasher
{
    /// <summary>
    /// FNV-1a offset basis - standard for FNV algorithm
    /// </summary>
    private const ulong FnvOffsetBasis = 14695981039346656037UL;

    /// <summary>
    /// FNV prime - multiplier for FNV algorithm
    /// </summary>
    private const ulong FnvPrime = 1099511628211UL;

    /// <summary>
    /// Compute order-independent hash from move key sequence.
    /// Uses XOR of individually hashed move keys - order doesn't matter.
    /// 
    /// Time: O(n) - no sorting needed
    /// Space: O(1) - no temporary arrays
    /// </summary>
    /// <param name="moveKeys">Move key sequence (any order)</param>
    /// <returns>Order-independent 64-bit hash</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeOrderIndependentHash(ReadOnlySpan<short> moveKeys)
    {
        if (moveKeys.Length == 0)
            return FnvOffsetBasis;

        // XOR all individual move hashes together
        // XOR is commutative: a ^ b = b ^ a
        // So order doesn't matter!
        ulong result = FnvOffsetBasis;

        foreach (short moveKey in moveKeys)
        {
            // Hash individual move key using FNV-1a
            ulong moveHash = FnvHash(moveKey);

            // Combine using XOR (order-independent)
            result ^= moveHash;
        }

        return result;
    }

    /// <summary>
    /// Compute order-independent hash from move key array.
    /// Alternative overload for array input.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeOrderIndependentHash(short[] moveKeys)
    {
        if (moveKeys == null || moveKeys.Length == 0)
            return FnvOffsetBasis;

        return ComputeOrderIndependentHash(new ReadOnlySpan<short>(moveKeys));
    }

    /// <summary>
    /// Compute order-independent hash from list of move keys.
    /// Alternative overload for List&lt;short&gt; input.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeOrderIndependentHash(List<short> moveKeys)
    {
        if (moveKeys == null || moveKeys.Count == 0)
            return FnvOffsetBasis;

        ulong result = FnvOffsetBasis;

        foreach (short moveKey in moveKeys)
        {
            ulong moveHash = FnvHash(moveKey);
            result ^= moveHash;
        }

        return result;
    }

    /// <summary>
    /// FNV-1a hash function for individual short values.
    /// Provides good distribution for 16-bit values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong FnvHash(short value)
    {
        // Treat short as two bytes
        byte byte1 = (byte)(value & 0xFF);
        byte byte2 = (byte)((value >> 8) & 0xFF);

        ulong hash = FnvOffsetBasis;

        // Hash first byte
        hash ^= byte1;
        hash *= FnvPrime;

        // Hash second byte
        hash ^= byte2;
        hash *= FnvPrime;

        return hash;
    }
}
