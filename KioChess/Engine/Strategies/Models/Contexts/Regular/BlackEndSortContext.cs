using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Regular;

public class BlackEndSortContext : BlackSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move)
    {
        MoveSorter.ProcessBlackEndCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move)
    {
        MoveSorter.ProcessBlackEndMove(move);
    }
}
