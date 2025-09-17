using Engine.DataStructures.Moves;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts;


public class SearchContext
{
    internal SearchResultType SearchResultType;

    internal int Value;
    internal int Ply;

    internal MoveHistoryList Moves;
    internal short BestMove;
    internal KillerMoves CurrentKillers;
    internal bool[] LowSee;
    public static MoveHistoryService MoveHistory;
    public static MoveProvider MoveProvider;

    public SearchContext()
    {
        Value = short.MinValue;
        Moves = new MoveHistoryList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Value = short.MinValue;
        Moves.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(short move)
    {
        CurrentKillers.Add(move);
        MoveHistory.SetCounterMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase GetMove(byte move) => MoveProvider.Get(Moves[move].Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase GetMove(int move) => MoveProvider.Get(Moves[move].Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool ShouldStore(sbyte depth, TranspositionEntry entry)
    {
        //if (TT == 1)
        //    return depth > entry.Depth || (depth == entry.Depth && (Value > entry.Value || (Value == entry.Value && BestMove != entry.PvMove)));

        //if (TT == 2)
        //    return depth > entry.Depth || (depth == entry.Depth && Value > entry.Value);

        return depth > entry.Depth;
    }
}