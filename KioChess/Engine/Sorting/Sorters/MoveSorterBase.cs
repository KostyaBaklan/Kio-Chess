using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public abstract class MoveSorterBase
{
    #region Pieces
    protected const byte WhitePawn = Pieces.WhitePawn;
    protected const byte WhiteKnight = Pieces.WhiteKnight;
    protected const byte WhiteBishop = Pieces.WhiteBishop;
    protected const byte WhiteRook = Pieces.WhiteRook;
    protected const byte WhiteQueen = Pieces.WhiteQueen;
    protected const byte WhiteKing = Pieces.WhiteKing;
    protected const byte BlackPawn = Pieces.BlackPawn;
    protected const byte BlackKnight = Pieces.BlackKnight;
    protected const byte BlackBishop = Pieces.BlackBishop;
    protected const byte BlackRook = Pieces.BlackRook;
    protected const byte BlackQueen = Pieces.BlackQueen;
    protected const byte BlackKing = Pieces.BlackKing;
    #endregion

    #region Squares
    protected const byte A1 = Squares.A1;
    protected const byte B1 = Squares.B1;
    protected const byte C1 = Squares.C1;
    protected const byte D1 = Squares.D1;
    protected const byte E1 = Squares.E1;
    protected const byte F1 = Squares.F1;
    protected const byte G1 = Squares.G1;
    protected const byte H1 = Squares.H1;
    protected const byte A2 = Squares.A2;
    protected const byte B2 = Squares.B2;
    protected const byte C2 = Squares.C2;
    protected const byte D2 = Squares.D2;
    protected const byte E2 = Squares.E2;
    protected const byte F2 = Squares.F2;
    protected const byte G2 = Squares.G2;
    protected const byte H2 = Squares.H2;
    protected const byte A3 = Squares.A3;
    protected const byte B3 = Squares.B3;
    protected const byte C3 = Squares.C3;
    protected const byte D3 = Squares.D3;
    protected const byte E3 = Squares.E3;
    protected const byte F3 = Squares.F3;
    protected const byte G3 = Squares.G3;
    protected const byte H3 = Squares.H3;
    protected const byte A4 = Squares.A4;
    protected const byte B4 = Squares.B4;
    protected const byte C4 = Squares.C4;
    protected const byte D4 = Squares.D4;
    protected const byte E4 = Squares.E4;
    protected const byte F4 = Squares.F4;
    protected const byte G4 = Squares.G4;
    protected const byte H4 = Squares.H4;
    protected const byte A5 = Squares.A5;
    protected const byte B5 = Squares.B5;
    protected const byte C5 = Squares.C5;
    protected const byte D5 = Squares.D5;
    protected const byte E5 = Squares.E5;
    protected const byte F5 = Squares.F5;
    protected const byte G5 = Squares.G5;
    protected const byte H5 = Squares.H5;
    protected const byte A6 = Squares.A6;
    protected const byte B6 = Squares.B6;
    protected const byte C6 = Squares.C6;
    protected const byte D6 = Squares.D6;
    protected const byte E6 = Squares.E6;
    protected const byte F6 = Squares.F6;
    protected const byte G6 = Squares.G6;
    protected const byte H6 = Squares.H6;
    protected const byte A7 = Squares.A7;
    protected const byte B7 = Squares.B7;
    protected const byte C7 = Squares.C7;
    protected const byte D7 = Squares.D7;
    protected const byte E7 = Squares.E7;
    protected const byte F7 = Squares.F7;
    protected const byte G7 = Squares.G7;
    protected const byte H7 = Squares.H7;
    protected const byte A8 = Squares.A8;
    protected const byte B8 = Squares.B8;
    protected const byte C8 = Squares.C8;
    protected const byte D8 = Squares.D8;
    protected const byte E8 = Squares.E8;
    protected const byte F8 = Squares.F8;
    protected const byte G8 = Squares.G8;
    protected const byte H8 = Squares.H8; 
    #endregion

    protected byte Phase;
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
    internal virtual MoveList GetOpeningMoves() => GetMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual MoveList GetBookOpeningMoves() => GetBookMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual MoveList GetMiddleMoves() => GetMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual MoveList GetBookMiddleMoves() => GetBookMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual MoveList GetEndMoves() => GetMoves();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual MoveList GetBookEndMoves() => GetBookMoves();
}