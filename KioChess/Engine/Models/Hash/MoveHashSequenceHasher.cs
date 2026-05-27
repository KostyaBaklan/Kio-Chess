using System.Runtime.CompilerServices;

namespace Engine.Models.Hash;

/// <summary>
/// Order-independent sequence hasher using pre-computed 128-bit MoveHash table
/// Pure XOR operation for maximum performance
/// </summary>
public class MoveHashSequenceHasher
{
    private static UInt128[] _moveHashes;
    private static bool _isInitialized = false;

    /// <summary>
    /// Initialize hasher with pre-computed move hashes from database
    /// Must be called once at startup before any hash computation
    /// </summary>
    /// <param name="moveHashes">Array of UInt128 hashes indexed by move key (short)</param>
    public static void Initialize(UInt128[] moveHashes)
    {
        if (_isInitialized)
        {
            Console.WriteLine("⚠ MoveHashSequenceHasher already initialized");
            return;
        }

        // Store pre-computed hash array directly
        _moveHashes = moveHashes;

        _isInitialized = true;
        Console.WriteLine($"✓ MoveHashSequenceHasher initialized with {_moveHashes.Length} pre-computed hashes");
    }

    /// <summary>
    /// Compute order-independent 128-bit hash for move sequence
    /// Uses XOR of pre-computed move hashes - order doesn't matter!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeSequenceHash(byte[] moveKeys)
    {
        int size = moveKeys.Length/2;

        UInt128 result = UInt128.Zero;

        for (int i = 0; i < moveKeys.Length; i+=2)
        {
            var key = BitConverter.ToInt16(moveKeys, i);
            result ^= _moveHashes[key];  // XOR is commutative - order independent!
        }

        return result;
    }

    /// <summary>
    /// Compute order-independent 128-bit hash for move sequence
    /// Uses XOR of pre-computed move hashes - order doesn't matter!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeSequenceHash(ReadOnlySpan<short> moveKeys)
    {
        UInt128 result = UInt128.Zero;

        for (int i = 0; i < moveKeys.Length; i++)
        {
            result ^= _moveHashes[moveKeys[i]];  // XOR is commutative - order independent!
        }

        return result;
    }

    /// <summary>
    /// Compute hash from string sequence (for migration from old PositionEntity)
    /// String stores move keys as Unicode characters
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeSequenceHashFromString(string sequence)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("MoveHashSequenceHasher not initialized. Call Initialize() first.");

        if (string.IsNullOrEmpty(sequence))
            return UInt128.Zero;

        UInt128 result = UInt128.Zero;

        for (int i = 0; i < sequence.Length; i++)
        {
            short moveKey = (short)sequence[i];
            result ^= _moveHashes[moveKey];
        }

        return result;
    }

    /// <summary>
    /// Array-based overload for convenience
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeSequenceHash(short[] moveKeys)
    {
        return ComputeSequenceHash(new ReadOnlySpan<short>(moveKeys));
    }

    /// <summary>
    /// Get pre-computed hash for a single move key
    /// Used for incremental/cumulative hash computation
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 GetMoveHash(short moveKey)
    {
        return _moveHashes[moveKey];
    }

    /// <summary>
    /// Check if hasher is initialized
    /// </summary>
    public static bool IsInitialized => _isInitialized;
}
