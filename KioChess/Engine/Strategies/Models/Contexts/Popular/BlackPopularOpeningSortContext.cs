using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public class BlackPopularOpeningSortContext : BlackPopularSortContext
{
    public BlackPopularOpeningSortContext()
    {
        Phase = Engine.Models.Enums.Phase.Opening;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessBlackOpeningCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessBlackOpeningMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveHistoryList GetBookMovesInternal() => MoveSorter.GetBookOpeningMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveHistoryList GetMovesInternal() => MoveSorter.GetOpeningMoves();
}
