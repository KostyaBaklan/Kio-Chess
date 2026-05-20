using Engine.DataStructures.Moves;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public sealed class WhiteBookEndSortContext : WhiteBookSortContext
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
    protected override void GetBookMovesInternal(ref MoveHistoryList moves) => MoveSorter.GetBookEndMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void GetMovesInternal(ref MoveHistoryList moves) => MoveSorter.GetEndMoves(ref moves);
}
