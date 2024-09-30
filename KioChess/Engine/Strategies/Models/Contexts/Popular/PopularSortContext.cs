using Engine.Dal.Models;
using Engine.DataStructures.Moves;
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

    internal override MoveHistoryList GetAllForEvaluation(Position position) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveHistoryList GetMoves()
    {
        Book.Reset();
        return GetBookMovesInternal();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveHistoryList GetAllMoves(Position position)
    {
        if (Moves == null)
            return GetAllBookMoves(position);

        var moveList = DataPoolService.GetCurrentMoveHistoryList();
        moveList.Clear();

        if (!HasPv)
        {
            for (int i = 0; i < Moves.Length; i++)
            {
                moveList.Add(new MoveHistory { Key = Moves[i].Key }); 
            }
        }
        else
        {
            var index = Array.FindIndex(Moves, m => m.Key == Pv);
            if (index > 0)
            {
                moveList.Add(new MoveHistory { Key = Moves[index].Key });
                for (int i = 0; i < Moves.Length; i++)
                {
                    if (i == index) continue;

                    moveList.Add(new MoveHistory { Key = Moves[i].Key });
                }
            }
            else
            {
                for (int i = 0; i < Moves.Length; i++)
                {
                    moveList.Add(new MoveHistory { Key = Moves[i].Key });
                }
            }
        }

        return moveList;
    }

    protected abstract MoveHistoryList GetAllBookMoves(Position position);

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
