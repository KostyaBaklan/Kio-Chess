using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IBoard
    {
        bool IsEmpty(BitBoard bitBoard);
        bool IsBlackPawn(BitBoard bitBoard);
        bool IsWhitePawn(BitBoard bitBoard);
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
        PositionCollection GetPiecePositions(byte index);
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
        BitBoard GetWhitePieceBits();
        BitBoard GetBlackPieceBits();
        BitBoard GetWhitePieceForKnightBits();
        BitBoard GetWhitePieceForBishopBits();
        BitBoard GetBlackPieceForKnightBits();
        BitBoard GetBlackPieceForBishopBits();
        BitBoard GetWhiteBitsForPawn();
        BitBoard GetBlackBitsForPawn();
        BitBoard GetBlackBitsForKnight();
        BitBoard GetWhiteBitsForKnight();
        BitBoard GetWhiteBitsForBishop();
        BitBoard GetBlackBitsForBishop();
        BitBoard GetBlackBits();
        BitBoard GetWhiteBits();
        Phase GetPhase();
        bool IsBlackPass(byte position);
        bool IsWhitePass(byte position);
        bool IsWhiteOver(BitBoard opponentPawns);
        bool IsBlackOver(BitBoard opponentPawns);
        bool IsDraw();
    }
}