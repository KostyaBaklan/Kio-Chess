using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Popular;

public abstract class WhitePopularSortContext : PopularSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void GetAllBookMoves(Position position, ref MoveHistoryList moveList) => position.GetAllWhiteBookMoves(this, ref moveList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessWhitePromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
}