using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public class WhitePopularOpeningSortContext : WhitePopularSortContext
{
    public WhitePopularOpeningSortContext()
    {
        Phase = Engine.Models.Enums.Phase.Opening;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteOpeningCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteOpeningMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void GetBookMovesInternal(ref MoveHistoryList moves) => MoveSorter.GetBookOpeningMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void GetMovesInternal(ref MoveHistoryList moves) => MoveSorter.GetOpeningMoves(ref moves);
}
