using Engine.Dal.Models;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public abstract class PopularSortContext : SortContext
{
    protected MoveHistory[] Moves;
    protected PopularMoves Book = PopularMoves.Default;

    public override bool IsRegular => Book.IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void GetMoves(ref MoveHistoryList moves)
    {
        Book.Reset();
        GetBookMovesInternal(ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void GetAllMoves(Position position, ref MoveHistoryList moveList)
    {
        if (Moves == null)
            GetAllBookMoves(position, ref moveList);
        else
        {
            var moves = Moves.AsSpan();
            if (!HasPv)
            {
                moveList.Add(moves);
            }
            else
            {
                var index = moves.FindIndex(Pv);
                if (index > 0)
                {
                    moveList.Add(moves[index]);
                    for (int i = 0; i < moves.Length; i++)
                    {
                        if (i == index) continue;

                        moveList.Add(moves[i]);
                    }
                }
                else
                {
                    moveList.Add(moves);
                }
            }
        }
    }

    protected abstract void  GetAllBookMoves(Position position, ref MoveHistoryList moveList);

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