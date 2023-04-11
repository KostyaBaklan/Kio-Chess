using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IBoard
    {
        bool IsEmpty(BitBoard bitBoard);
        bool CanDoBlackSmallCastle();
        bool CanDoWhiteSmallCastle();
        bool CanDoBlackBigCastle();
        bool CanDoWhiteBigCastle();
        bool IsWhiteOpposite(byte square);
        bool IsBlackOpposite(byte square);
        short GetValue();
        int GetStaticValue();
        int GetKingSafetyValue();
        Piece GetPiece(byte cell);
        bool GetPiece(byte cell, out Piece? piece);
        void DoWhiteSmallCastle();
        void DoBlackSmallCastle();
        void DoBlackBigCastle();
        void DoWhiteBigCastle();
        void UndoWhiteSmallCastle();
        void UndoBlackSmallCastle();
        void UndoWhiteBigCastle();
        void UndoBlackBigCastle();
        void Remove(Piece victim, byte square);
        void Add(Piece victim, byte square);
        void Move(Piece piece, byte from, byte to);
        byte GetWhiteKingPosition();
        byte GetBlackKingPosition();
        int GetPawnValue();
        ulong GetKey();
        PositionsList GetPiecePositions(byte index);
        void GetSquares(byte p, SquareList squares);
        BitBoard GetOccupied();
        BitBoard GetPieceBits(Piece piece);
        BitBoard GetPerimeter();
        byte UpdatePhase();
        int StaticExchange(AttackBase attack);
        int GetBlackMaxValue();
        int GetWhiteMaxValue();
        bool CanWhitePromote();
        bool CanBlackPromote();
        BitBoard GetWhitePawnAttacks();
        BitBoard GetBlackPawnAttacks();
        BitBoard GetRank(int rank);
        byte GetPhase();
        bool IsBlackPass(byte position);
        bool IsWhitePass(byte position);
        bool IsWhiteOver(BitBoard opponentPawns);
        bool IsBlackOver(BitBoard opponentPawns);
        bool IsDraw();
        void GetWhitePromotionSquares(SquareList promotionSquares);
        void GetBlackPromotionSquares(SquareList promotionSquares);
        void GetWhitePawnSquares(SquareList squareList);
        void GetBlackPawnSquares(SquareList squareList);
        bool IsBlackAttacksTo(byte position);
        bool IsWhiteAttacksTo(byte to);
        bool IsBlockedByBlack(byte position);
        bool IsBlockedByWhite(byte position);
    }
}