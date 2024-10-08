﻿using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public abstract class WhitePopularSortContext : PopularSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetAllAttacks(Position position) => position.GetAllWhiteAttacks(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetAllBookMoves(Position position) => position.GetAllWhiteBookMoves(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessWhitePromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
}
