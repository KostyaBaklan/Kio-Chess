using Engine.DataStructures.Moves.Lists;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures.Moves;

public struct MoveHistoryList
{
    public static byte One = 1;
    public static byte Zero = 0;

    public MoveHistoryBuffer Moves;
    public byte Count;
    public byte LmrIndex;

    public MoveHistoryList()
    {
        Moves = new MoveHistoryBuffer();
        Count = Zero;
        LmrIndex = Zero;
    }

    public MoveHistory this[byte index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Moves[index];
    }

    public MoveHistory this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Moves[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(MoveHistory move) => Moves[Count++] = move;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Count = Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(Span<MoveHistory> moves)
    {
        var span = MemoryMarshal.CreateSpan(ref Moves[Count], moves.Length);
        moves.CopyTo(span);
        Count += (byte)moves.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionAttackList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            Moves[Count++] = new MoveHistory(moves[i].Key, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            Moves[Count++] = new MoveHistory(moves[i].Key, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionList moves, int attackValue)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            Moves[Count++] = new MoveHistory(moves[i].Key, attackValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionAttackList moves, int attackValue)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            Moves[Count++] = new MoveHistory(moves[i].Key, attackValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(ref MoveHistoryList moves)
    {
        ref var sourceRef = ref MemoryMarshal.GetReference(moves.AsSpan());
        ref var destRef = ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Moves[Count], moves.Count));
        for (byte i = Zero; i < moves.Count; i++)
        {
            Unsafe.Add(ref destRef, i) = Unsafe.Add(ref sourceRef, i);
        }
        Count += moves.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<MoveHistory> AsSpan()
    {
        return MemoryMarshal.CreateSpan(ref Moves[0], Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void CopyClear(ref MoveHistoryList moves)
    {
        if (moves.Count == Zero) return;

        Add(ref moves);
        moves.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SortCopyClear(ref MoveHistoryList moves)
    {
        if (moves.Count == Zero) return;

        if (moves.Count > One)
            moves.Sort();

        Add(ref moves);
        moves.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Sort()
    {
        ref MoveHistory keysRef = ref Moves[0];

        for (int i = One; i < Count; i++)
        {
            MoveHistory key = Unsafe.Add(ref keysRef, i);
            int j = i - 1;

            while (j >= 0 && key.IsGreater(Unsafe.Add(ref keysRef, j)))
            {
                Unsafe.Add(ref keysRef, j + 1) = Unsafe.Add(ref keysRef, j);
                j--;
            }
            Unsafe.Add(ref keysRef, j + 1) = key;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Insert(MoveHistory move)
    {
        byte position = Count;

        Add(move);

        byte parent = Parent(position);

        while (position > Zero && Moves[position].IsGreater(Moves[parent]))
        {
            Swap(ref Moves[position], ref Moves[parent]);
            position = parent;
            parent = Parent(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Parent(byte i) => (byte)((i - One) >> 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap(ref MoveHistory a, ref MoveHistory b) => (a, b) = (b, a);
}