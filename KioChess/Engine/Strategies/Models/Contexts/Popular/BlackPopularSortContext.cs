using Engine.DataStructures.Moves.Lists;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public abstract class BlackPopularSortContext : PopularSortContext
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions)
    {
        MoveSorter.ProcessBlackPromotionMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList)
    {
        MoveSorter.ProcessBlackPromotionCaptures(promotionAttackList);
    }
}
