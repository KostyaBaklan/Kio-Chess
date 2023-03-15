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
        bool IsWhiteOpposite(Square square);
        bool IsBlackOpposite(Square square);
        short GetValue();
        int GetStaticValue();
        int GetKingSafetyValue();
        Piece GetPiece(Square cell);
        bool GetPiece(Square cell, out Piece? piece);
        void DoWhiteSmallCastle();
        void DoBlackSmallCastle();
        void DoBlackBigCastle();
        void DoWhiteBigCastle();
        void UndoWhiteSmallCastle();
        void UndoBlackSmallCastle();
        void UndoWhiteBigCastle();
        void UndoBlackBigCastle();
        void Remove(Piece victim, Square square);
        void Add(Piece victim, Square square);
        void Move(Piece piece, Square from,Square to);
        byte GetWhiteKingPosition();
        byte GetBlackKingPosition();
        int GetPawnValue();
        ulong GetKey();
        PositionsList GetPiecePositions(byte index);
        void GetSquares(byte p, SquareList squares);
        BitBoard GetOccupied();
        BitBoard GetPieceBits(Piece piece);
        BitBoard GetPerimeter();
        Phase UpdatePhase();
        int StaticExchange(AttackBase attack);
        int GetBlackMaxValue();
        int GetWhiteMaxValue();
        bool CanWhitePromote();
        bool CanBlackPromote();
        BitBoard GetWhitePawnAttacks();
        BitBoard GetBlackPawnAttacks();
        BitBoard GetRank(int rank);
        Phase GetPhase();
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
        bool IsBlockedByBlack(int position);
        bool IsBlockedByWhite(int position);
    }
}