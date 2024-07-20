using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class WhiteBookMiddleSortContext : WhiteBookSortContext
{
    public WhiteBookMiddleSortContext()
    {
        Phase = Engine.Models.Enums.Phase.Middle;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteMiddleCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteMiddleMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetBookMovesInternal() => MoveSorter.GetBookMiddleMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetMovesInternal() => MoveSorter.GetMiddleMoves();
}
