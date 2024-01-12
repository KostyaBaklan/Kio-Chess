using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class WhiteBookOpeningSortContext : WhiteBookSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteOpeningCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteOpeningMove(move);
}
