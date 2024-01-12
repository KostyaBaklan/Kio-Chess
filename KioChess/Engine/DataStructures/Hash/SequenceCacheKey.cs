using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Hash
{
    public readonly struct SequenceCacheKey:IEquatable<SequenceCacheKey>
    {
        public readonly ulong Board;
        public readonly bool Flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceCacheKey(ulong board, bool flag)
        {
            Board= board;
            Flag= flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SequenceCacheKey other) => Board == other.Board && Flag == other.Flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Board.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is SequenceCacheKey && Equals((SequenceCacheKey)obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SequenceCacheKey left, SequenceCacheKey right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SequenceCacheKey left, SequenceCacheKey right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{Board}-{Flag}";
    }
}
