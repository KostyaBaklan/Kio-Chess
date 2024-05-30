using Engine.Dal.Models;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public abstract class PopularSortContext : SortContext
{
    protected MoveBase[] Moves;
    protected PopularMoves Book = PopularMoves.Default;

    public override bool IsRegular => Book.IsEmpty;

    internal override MoveList GetAllForEvaluation(Position position) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetMoves()
    {
        Book.Reset();
        return MoveSorter.GetBookMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetAllMoves(Position position)
    {
        if (Moves == null)
            return GetAllBookMoves(position);

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

    protected abstract MoveList GetAllBookMoves(Position position);

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

        GetCachedMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Set(MoveSorterBase sorter, short pv)
    {
        SetInternal(sorter, pv);

        GetCachedMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetCachedMoves()
    {
        Moves = MoveHistory.GetCachedMoves();

        if (Moves != null)
            return;

        Book = MoveHistory.GetBook();
        Book.SetMoves();
    }
}
