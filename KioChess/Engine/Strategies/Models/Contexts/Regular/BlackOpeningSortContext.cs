﻿using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Regular;

public class BlackOpeningSortContext : BlackSortContext
{
    public BlackOpeningSortContext()
    {
        Phase = Engine.Models.Enums.Phase.Opening;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessBlackOpeningCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessBlackOpeningMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetBookMovesInternal() => MoveSorter.GetBookOpeningMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetMovesInternal() => MoveSorter.GetOpeningMoves();
}
