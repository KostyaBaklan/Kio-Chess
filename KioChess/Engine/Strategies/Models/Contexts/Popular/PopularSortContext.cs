using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public abstract class PopularSortContext : SortContext
{
    protected MoveBase[] Moves;
    protected PopularMoves Book = PopularMoves.Default;

    public override bool IsRegular => Book.IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetMoves()
    {
        Book.Reset();
        return MoveSorter.GetBookMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetAllMoves(IPosition position)
    {
        if (Moves == null)
            return position.GetAllBookMoves(this);

        var moveList = DataPoolService.GetCurrentMoveList();
        moveList.Clear();

        if (!HasPv)
        {
            moveList.Add(Moves);
        }
        else
        {
            var index = Array.FindIndex(Moves, m => m.Key == Pv);
            if (index > 0)
            {
                moveList.Add(Moves[index]);
                for (int i = 0; i < Moves.Length; i++)
                {
                    if (i == index) continue;

                    moveList.Add(Moves[i]);
                }
            }
            else
            {
                moveList.Add(Moves);
            }
        }

        return moveList;
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

        Moves = MoveHistory.GetCachedMoves();

        if (Moves != null)
            return;

        Book = MoveHistory.GetBook();
        Book.SetMoves();
    }
}
