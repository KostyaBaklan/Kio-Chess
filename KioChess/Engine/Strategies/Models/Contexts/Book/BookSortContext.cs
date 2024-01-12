using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public abstract class BookSortContext : SortContext
{
    protected PopularMoves Book = PopularMoves.Default;

    public override bool IsRegular => Book.IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetMoves()
    {
        Book.Reset();
        return MoveSorter.GetBookMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsRegularMove(MoveBase move)
    {
        if (!Book.IsPopular(move))
            return true;

        MoveSorter.AddSuggestedBookMove(move);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Set(MoveSorterBase sorter, MoveBase pv = null)
    {
        SetInternal(sorter, pv);

        Book = MoveHistory.GetBook();
        Book.SetMoves();
    }
}
