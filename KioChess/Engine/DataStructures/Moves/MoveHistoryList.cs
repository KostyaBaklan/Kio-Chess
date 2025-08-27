using Engine.DataStructures.Moves.Lists;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves;

public struct MoveHistoryList
{
    public static byte One = 1;
    public static byte Zero = 0;

    public MoveHistoryBuffer Moves;
    public byte Count;

    public MoveHistoryList()
    {
        Moves = new MoveHistoryBuffer();
        Count = Zero;
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
        for (byte i = Zero; i < moves.Length; i++)
        {
            Moves[Count++] = moves[i];
        }
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
        for (byte i = Zero; i < moves.Count; i++)
        {
            Moves[Count++] = moves[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void CopyClear(ref MoveHistoryList moves)
    {
        if(moves.Count == Zero) return;

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
        for (byte i = One; i < Count; i++)
        {
            var key = Moves[i];
            int j = i - One;

            while (j > -1 && key.IsGreater(Moves[j]))
            {
                Moves[j + One] = Moves[j];
                j--;
            }
            Moves[j + One] = key;
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
    private byte Parent(byte i) => (byte)((i - One) / 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap(ref MoveHistory a, ref MoveHistory b) => (a, b) = (b, a);
}