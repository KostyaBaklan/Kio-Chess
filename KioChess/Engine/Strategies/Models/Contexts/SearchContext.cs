using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts;


public class SearchContext
{
    internal SearchResultType SearchResultType;

    internal int Value;
    internal int Ply;

    internal MoveList Moves;
    internal MoveBase BestMove;
    internal KillerMoves CurrentKillers;
    public static IMoveHistoryService MoveHistory;

    public SearchContext()
    {
        Value = short.MinValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Value = short.MinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(short move)
    {
        CurrentKillers.Add(move);
        MoveHistory.SetCounterMove(move);
    }
}
