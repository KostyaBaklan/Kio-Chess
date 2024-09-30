using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public abstract class BlackBookSortContext : BookSortContext
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveHistoryList GetAllMoves(Position position) => position.GetAllBlackBookMoves(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveHistoryList GetAllAttacks(Position position) => position.GetAllBlackAttacks(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessBlackPromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessBlackPromotionCaptures(promotionAttackList);
}
