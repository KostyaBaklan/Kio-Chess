using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts;


public class SearchContext
{
    internal SearchResultType SearchResultType;


    internal short Value;
    internal int Ply;

    internal MoveValueList Moves;
    internal MoveBase BestMove;

    public SearchContext()
    {
        Value = short.MinValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Value = short.MinValue;
    }
}
