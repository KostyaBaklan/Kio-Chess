using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class WhiteBookEndSortContext : WhiteBookSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteEndCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteEndMove(move);
}
