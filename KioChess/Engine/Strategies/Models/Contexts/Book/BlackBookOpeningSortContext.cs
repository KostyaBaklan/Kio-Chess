﻿using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public class BlackBookOpeningSortContext : BlackBookSortContext
{
    public BlackBookOpeningSortContext()
    {
        Phase = Engine.Models.Enums.Phase.Opening;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessBlackOpeningCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessBlackOpeningMove(move);
}
