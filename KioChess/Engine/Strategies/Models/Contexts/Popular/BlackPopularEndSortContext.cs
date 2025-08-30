using Engine.DataStructures.Moves;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public class BlackPopularEndSortContext : BlackPopularSortContext
{
    public BlackPopularEndSortContext()
    {
        Phase = Engine.Models.Enums.Phase.End;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessBlackEndCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessBlackEndMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void GetBookMovesInternal(ref MoveHistoryList moves) => MoveSorter.GetBookEndMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void GetMovesInternal(ref MoveHistoryList moves) => MoveSorter.GetEndMoves(ref moves);
}
