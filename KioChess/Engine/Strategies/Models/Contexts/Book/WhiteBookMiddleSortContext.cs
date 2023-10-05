using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class WhiteBookMiddleSortContext : WhiteBookSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move)
    {
        MoveSorter.ProcessWhiteMiddleCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move)
    {
        MoveSorter.ProcessWhiteMiddleMove(move);
    }
}
