using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Regular;

public class BlackMiddleSortContext : BlackSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move)
    {
        MoveSorter.ProcessBlackMiddleCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move)
    {
        MoveSorter.ProcessBlackMiddleMove(move);
    }
}
