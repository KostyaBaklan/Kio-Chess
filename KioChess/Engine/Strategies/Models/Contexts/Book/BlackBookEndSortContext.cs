using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class BlackBookEndSortContext : BlackBookSortContext
{
    public BlackBookEndSortContext()
    {
        Phase = Engine.Models.Enums.Phase.End;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessBlackEndCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessBlackEndMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetBookMovesInternal() => MoveSorter.GetBookEndMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetMovesInternal() => MoveSorter.GetEndMoves();
}
