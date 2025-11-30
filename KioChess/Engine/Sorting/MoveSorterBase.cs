using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Sorting;

public abstract class MoveSorterBase
{
    //protected byte Phase;
    protected int StaticValue;
    protected readonly AttackList attackList;
    protected readonly MoveHistoryService MoveHistoryService;
    protected readonly Position Position;
    protected readonly MoveList EmptyList;

    protected readonly Board Board;
    protected readonly MoveProvider MoveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
    protected readonly DataPoolService DataPoolService = ContainerLocator.Current.Resolve<DataPoolService>();
    protected readonly IConfigurationProvider ConfigurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();

    protected MoveSorterBase(Position position)
    {
        EmptyList = new MoveList(0);
        attackList = [];
        Board = position.GetBoard();
        Position = position;

        MoveHistoryService = ContainerLocator.Current.Resolve<MoveHistoryService>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetCounterMove() => MoveHistoryService.GetCounterMove();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetCountermoveHistoryMove() => MoveHistoryService.GetCountermoveHistory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessHashMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessKillerMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessCounterMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessCountermoveHistoryMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessCaptureMove(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhiteOpeningMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhiteMiddleMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhiteEndMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackOpeningMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackMiddleMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackEndMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhitePromotionMoves(PromotionList promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackPromotionMoves(PromotionList promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhitePromotionCaptures(PromotionAttackList promotionAttackList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackPromotionCaptures(PromotionAttackList promotionAttackList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessHashMoves(PromotionList promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessHashMoves(PromotionAttackList promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhiteOpeningCapture(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhiteMiddleCapture(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessWhiteEndCapture(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackOpeningCapture(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackMiddleCapture(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessBlackEndCapture(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void AddSuggestedBookMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual void SetValues() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void GetMoves(ref MoveHistoryList moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void GetBookMoves(ref MoveHistoryList moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual void GetOpeningMoves(ref MoveHistoryList moves) => GetMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual void GetBookOpeningMoves(ref MoveHistoryList moves) => GetBookMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual void GetMiddleMoves(ref MoveHistoryList moves) => GetMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual void GetBookMiddleMoves(ref MoveHistoryList moves) => GetBookMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual void GetEndMoves(ref MoveHistoryList moves) => GetMoves(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual void GetBookEndMoves(ref MoveHistoryList moves) => GetBookMoves(ref moves);
}