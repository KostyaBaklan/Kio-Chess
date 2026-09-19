using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Common;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public abstract class WhiteBookSortContext : BookSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void GetAllMoves(Position position, ref MoveHistoryList moves)
    {
        position.GetAllBookMoves<WhiteColor>(this, ref moves);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessWhitePromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
}