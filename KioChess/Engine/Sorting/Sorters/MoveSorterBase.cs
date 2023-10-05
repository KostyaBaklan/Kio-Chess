using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters;

public abstract class MoveSorterBase
{
    protected const byte WhitePawn = 0;
    protected const byte WhiteKnight = 1;
    protected const byte WhiteBishop = 2;
    protected const byte WhiteRook = 3;
    protected const byte WhiteQueen = 4;
    protected const byte WhiteKing = 5;
    protected const byte BlackPawn = 6;
    protected const byte BlackKnight = 7;
    protected const byte BlackBishop = 8;
    protected const byte BlackRook = 9;
    protected const byte BlackQueen = 10;
    protected const byte BlackKing = 11;

    protected const byte A1 = 0;
    protected const byte B1 = 1;
    protected const byte C1 = 2;
    protected const byte D1 = 3;
    protected const byte E1 = 4;
    protected const byte F1 = 5;
    protected const byte G1 = 6;
    protected const byte H1 = 7;
    protected const byte A2 = 8;
    protected const byte B2 = 9;
    protected const byte C2 = 10;
    protected const byte D2 = 11;
    protected const byte E2 = 12;
    protected const byte F2 = 13;
    protected const byte G2 = 14;
    protected const byte H2 = 15;
    protected const byte A3 = 16;
    protected const byte B3 = 17;
    protected const byte C3 = 18;
    protected const byte D3 = 19;
    protected const byte E3 = 20;
    protected const byte F3 = 21;
    protected const byte G3 = 22;
    protected const byte H3 = 23;
    protected const byte A4 = 24;
    protected const byte B4 = 25;
    protected const byte C4 = 26;
    protected const byte D4 = 27;
    protected const byte E4 = 28;
    protected const byte F4 = 29;
    protected const byte G4 = 30;
    protected const byte H4 = 31;
    protected const byte A5 = 32;
    protected const byte B5 = 33;
    protected const byte C5 = 34;
    protected const byte D5 = 35;
    protected const byte E5 = 36;
    protected const byte F5 = 37;
    protected const byte G5 = 38;
    protected const byte H5 = 39;
    protected const byte A6 = 40;
    protected const byte B6 = 41;
    protected const byte C6 = 42;
    protected const byte D6 = 43;
    protected const byte E6 = 44;
    protected const byte F6 = 45;
    protected const byte G6 = 46;
    protected const byte H6 = 47;
    protected const byte A7 = 48;
    protected const byte B7 = 49;
    protected const byte C7 = 50;
    protected const byte D7 = 51;
    protected const byte E7 = 52;
    protected const byte F7 = 53;
    protected const byte G7 = 54;
    protected const byte H7 = 55;
    protected const byte A8 = 56;
    protected const byte B8 = 57;
    protected const byte C8 = 58;
    protected const byte D8 = 59;
    protected const byte E8 = 60;
    protected const byte F8 = 61;
    protected const byte G8 = 62;
    protected const byte H8 = 63;

    protected readonly IKillerMoveCollection[] Moves;
    protected readonly AttackList attackList;
    protected readonly IMoveHistoryService MoveHistoryService;
    protected IMoveComparer Comparer;
    protected IKillerMoveCollection CurrentKillers;
    protected readonly IPosition Position;
    protected readonly MoveList EmptyList;

    protected readonly IBoard Board;
    protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
    protected readonly IDataPoolService DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();

    protected MoveSorterBase(IPosition position, IMoveComparer comparer)
    {
        EmptyList = new MoveList(0);
        attackList = new AttackList();
        Board = position.GetBoard();
        Comparer = comparer;
        Moves = ServiceLocator.Current.GetInstance<IKillerMoveCollectionFactory>().CreateMoves();
        Position = position;

        MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetCounterMove()
    {
        return MoveHistoryService.GetCounterMove();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Add(short move)
    {
        Moves[MoveHistoryService.GetPly()].Add(move);
        MoveHistoryService.SetCounterMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessHashMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessKillerMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessCounterMove(MoveBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract void ProcessCaptureMove(AttackBase move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract MoveList GetMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal abstract MoveList GetBookMoves();

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
    internal bool IsKiller(short key)
    {
        return CurrentKillers.Contains(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetKillers()
    {
        CurrentKillers = Moves[MoveHistoryService.GetPly()];
    }

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
}