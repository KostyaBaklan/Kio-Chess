using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public abstract class BookSortContext : SortContext
{
    protected MoveBase[] Moves;
    protected IPopularMoves Book;
    protected static IPopularMoves _defaultValue = new PopularMoves0();

    public override bool IsRegular => Book.IsEmpty;

    public override bool HasMoves => Moves!=null;

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
        MoveSorter = sorter;
        MoveSorter.SetKillers();
        CounterMove = sorter.GetCounterMove();

        if (pv != null)
        {
            HasPv = true;
            Pv = pv.Key;
            IsPvCapture = pv.IsAttack;
        }
        else
        {
            HasPv = false;
        }

        Moves = MoveHistory.GetCachedMoves();

        if (Moves == null)
        {
            Book = MoveHistory.GetBook();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetCachedMoves()
    {
        MoveList moves = new MoveList(Moves.Length)
        {
            Moves
        };

        //moves.FullSort();
        return moves;
    }
}
