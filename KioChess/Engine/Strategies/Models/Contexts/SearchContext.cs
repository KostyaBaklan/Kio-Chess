using Engine.DataStructures.Moves;
using Engine.Models.Moves;
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
        Moves.LmrIndex = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(short move)
    {
        CurrentKillers.Add(move);
        MoveHistory.SetCounterMove(move);
        MoveHistory.SetCountermoveHistory(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase GetMove(byte move) => MoveProvider.Get(Moves[move].Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase GetMove(int move) => MoveProvider.Get(Moves[move].Key);
}