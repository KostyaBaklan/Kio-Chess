using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Sorting;

public abstract class MoveSorterBase
{
    protected static byte Zero = 0;

    protected readonly BitBoard _minorStartPositions;
    protected readonly BitBoard _perimeter;

    protected int StaticValue;
    protected readonly AttackList attackList;
    protected readonly MoveHistoryService MoveHistoryService;
    protected readonly Position Position;
    protected readonly MoveCollection MoveCollection;

    protected readonly Board Board;
    protected readonly MoveProvider MoveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
    protected readonly DataPoolService DataPoolService = ContainerLocator.Current.Resolve<DataPoolService>();
    protected readonly IConfigurationProvider ConfigurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();

    protected MoveSorterBase(Position position)
    {
        MoveCollection = new MoveCollection();
        attackList = [];
        Board = position.GetBoard();
        Position = position;

        _minorStartPositions = Squares.B1.AsBitBoard() | Squares.C1.AsBitBoard() | Squares.F1.AsBitBoard() |
                               Squares.G1.AsBitBoard() | Squares.B8.AsBitBoard() | Squares.C8.AsBitBoard() |
                               Squares.F8.AsBitBoard() | Squares.G8.AsBitBoard();
        _perimeter = Board.GetPerimeter();

        MoveHistoryService = ContainerLocator.Current.Resolve<MoveHistoryService>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessHashMove(MoveBase move) => MoveCollection.AddHashMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessKillerMove(MoveBase move) => MoveCollection.AddKillerMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessCounterMove(MoveBase move) => MoveCollection.AddCounterMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessCountermoveHistoryMove(MoveBase move) => MoveCollection.AddCountermoveHistory(move);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //internal abstract void ProcessCaptureMove(AttackBase move);

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
    internal void ProcessHashMoves(PromotionList promotions) => MoveCollection.AddHashMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessHashMoves(PromotionAttackList promotions) => MoveCollection.AddHashMoves(promotions);

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
    internal void AddSuggestedBookMove(MoveBase move) => MoveCollection.AddSuggestedBookMove(move);

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