﻿using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public class WhitePopularEndSortContext : WhitePopularSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessCaptureMove(AttackBase move) => MoveSorter.ProcessWhiteEndCapture(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessMove(MoveBase move) => MoveSorter.ProcessWhiteEndMove(move);
}
