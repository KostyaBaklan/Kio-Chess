using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public abstract class BookSortContext : SortContext
{
    protected PopularMoves Book = PopularMoves.Default;

    public override bool IsRegular => Book.IsEmpty;

    internal override MoveList GetAllForEvaluation(Position position) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetMoves()
    {
        if (Book.IsEmpty) return GetMovesInternal();

        Book.Reset();
        return GetBookMovesInternal();
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
    public override void Set(MoveSorterBase sorter)
    {
        SetInternal(sorter);

        SetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Set(MoveSorterBase sorter, short pv)
    {
        SetInternal(sorter, pv);

        SetMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetMoves()
    {
        Book = MoveHistory.GetBook();
        Book.SetMoves();
    }
}
