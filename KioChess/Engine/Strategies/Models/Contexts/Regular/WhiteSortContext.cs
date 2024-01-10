using Engine.DataStructures.Moves.Lists;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Regular;

public abstract class WhiteSortContext : RegularSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessWhitePromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
}
