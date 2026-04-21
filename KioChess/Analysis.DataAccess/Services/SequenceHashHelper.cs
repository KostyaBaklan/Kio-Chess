using System.Runtime.CompilerServices;

namespace Analysis.DataAccess.Services;

/// <summary>
/// Helper methods for working with move sequences and sequence hashes.
/// Provides conversion and hashing utilities for the Opening Explorer.
/// </summary>
public static class SequenceHashHelper
{
    /// <summary>
    /// Convert move key array to its order-independent hash.
    /// Uses OrderIndependentSequenceHasher for fast, order-independent hashing.
    /// 
    /// Time: O(n) - no sorting needed
    /// Space: O(1) - no temporary storage
    /// </summary>
    /// <param name="moveKeys">Move keys in any order</param>
    /// <returns>Order-independent 64-bit hash value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeSequenceHash(short[] moveKeys)
    {
        return OrderIndependentSequenceHasher.ComputeOrderIndependentHash(moveKeys);
    }

    /// <summary>
    /// Convert move key list to its order-independent hash.
    /// </summary>
    /// <param name="moveKeys">List of move keys in any order</param>
    /// <returns>Order-independent 64-bit hash value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeSequenceHash(List<short> moveKeys)
    {
        return OrderIndependentSequenceHasher.ComputeOrderIndependentHash(moveKeys);
    }

    /// <summary>
    /// Convert move key span to its order-independent hash.
    /// </summary>
    /// <param name="moveKeys">Span of move keys in any order</param>
    /// <returns>Order-independent 64-bit hash value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeSequenceHash(ReadOnlySpan<short> moveKeys)
    {
        return OrderIndependentSequenceHasher.ComputeOrderIndependentHash(moveKeys);
    }

    /// <summary>
    /// Compute sequence hash from move keys and return both hash and move keys.
    /// Utility method for creating OpeningEntry with computed values.
    /// </summary>
    /// <param name="moveKeys">Move key array in any order</param>
    /// <returns>Tuple of (moveKeys, sequenceHash)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (short[] MoveKeys, ulong SequenceHash) ComputeWithHash(short[] moveKeys)
    {
        var hash = OrderIndependentSequenceHasher.ComputeOrderIndependentHash(moveKeys);
        return (moveKeys, hash);
    }

    /// <summary>
    /// Verify that two different move key orders produce the same hash.
    /// Useful for testing and debugging order-independence.
    /// </summary>
    /// <param name="keys1">First move key sequence</param>
    /// <param name="keys2">Second move key sequence (different order)</param>
    /// <returns>True if both produce same hash, false otherwise</returns>
    public static bool VerifyOrderIndependence(short[] keys1, short[] keys2)
    {
        if (keys1 == null || keys2 == null)
            return keys1 == keys2;

        if (keys1.Length != keys2.Length)
            return false;

        var hash1 = OrderIndependentSequenceHasher.ComputeOrderIndependentHash(keys1);
        var hash2 = OrderIndependentSequenceHasher.ComputeOrderIndependentHash(keys2);

        return hash1 == hash2;
    }
}
