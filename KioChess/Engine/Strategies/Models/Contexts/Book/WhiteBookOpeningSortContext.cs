using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class WhiteBookOpeningSortContext : WhiteBookSortContext
{
    public WhiteBookOpeningSortContext()
    {
        Phase = Engine.Models.Enums.Phase.Opening;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteOpeningCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteOpeningMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveHistoryList GetBookMovesInternal() => MoveSorter.GetBookOpeningMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveHistoryList GetMovesInternal() => MoveSorter.GetOpeningMoves();
}
