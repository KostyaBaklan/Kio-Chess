using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public class WhitePopularMiddleSortContext : WhitePopularSortContext
{
    public WhitePopularMiddleSortContext()
    {
        Phase = Engine.Models.Enums.Phase.Middle;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteMiddleCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteMiddleMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveHistoryList GetBookMovesInternal() => MoveSorter.GetBookMiddleMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveHistoryList GetMovesInternal() => MoveSorter.GetMiddleMoves();
}
