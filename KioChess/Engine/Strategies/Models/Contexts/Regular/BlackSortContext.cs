using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Regular;

public abstract class BlackSortContext : RegularSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetAllAttacks(IPosition position) => position.GetAllBlackAttacks(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetAllMoves(IPosition position) => position.GetAllBlackMoves(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessBlackPromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessBlackPromotionCaptures(promotionAttackList);
}
