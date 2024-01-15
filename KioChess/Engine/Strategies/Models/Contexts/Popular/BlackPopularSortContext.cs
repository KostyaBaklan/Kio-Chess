using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public abstract class BlackPopularSortContext : PopularSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetAllAttacks(Position position) => position.GetAllBlackAttacks(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override MoveList GetAllBookMoves(Position position) => position.GetAllBlackBookMoves(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessBlackPromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessBlackPromotionCaptures(promotionAttackList);
}
