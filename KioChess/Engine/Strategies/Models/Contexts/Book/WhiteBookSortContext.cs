using Engine.DataStructures.Moves.Lists;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public abstract class WhiteBookSortContext : BookSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions)
    {
        MoveSorter.ProcessWhitePromotionMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList)
    {
        MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
    }
}
