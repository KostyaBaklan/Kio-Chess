using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class WhiteBookEndSortContext : WhiteBookSortContext
{
    public WhiteBookEndSortContext()
    {
        Phase = Engine.Models.Enums.Phase.End;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteEndCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteEndMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetBookMovesInternal()
    {
        return MoveSorter.GetBookEndMoves();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetMovesInternal()
    {
        return MoveSorter.GetEndMoves();
    }
}
