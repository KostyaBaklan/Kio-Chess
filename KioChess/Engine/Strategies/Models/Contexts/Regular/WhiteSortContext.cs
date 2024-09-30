using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Regular;

public abstract class WhiteSortContext : RegularSortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveHistoryList GetAllForEvaluation(Position position) => Position.GetAllWhiteForEvaluation(this);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveHistoryList GetAllAttacks(Position position) => position.GetAllWhiteAttacks(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveHistoryList GetAllMoves(Position position) => position.GetAllWhiteMoves(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessWhitePromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
}
