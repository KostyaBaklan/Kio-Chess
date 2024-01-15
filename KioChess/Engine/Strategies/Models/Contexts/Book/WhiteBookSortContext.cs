using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Book;

public abstract class WhiteBookSortContext : BookSortContext
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override MoveList GetAllMoves(Position position) => position.GetAllWhiteBookMoves(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetAllAttacks(Position position) => position.GetAllWhiteAttacks(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionMoves(PromotionList promotions) => MoveSorter.ProcessWhitePromotionMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList) => MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
}
