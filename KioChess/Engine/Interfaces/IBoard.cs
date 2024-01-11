﻿using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Moves;

namespace Engine.Interfaces;

public interface IBoard
{
    bool IsEmpty(BitBoard bitBoard);
    bool CanDoBlackSmallCastle();
    bool CanDoWhiteSmallCastle();
    bool CanDoBlackBigCastle();
    bool CanDoWhiteBigCastle();
    bool IsWhiteOpposite(byte square);
    bool IsBlackOpposite(byte square);
    int Evaluate();
    int EvaluateOpposite();
    int GetStaticValue();
    int GetKingSafetyValue();
    byte GetPiece(byte cell);
    bool GetPiece(byte cell, out byte? piece);
    void DoWhiteSmallCastle();
    void DoBlackSmallCastle();
    void DoBlackBigCastle();
    void DoWhiteBigCastle();
    void UndoWhiteSmallCastle();
    void UndoBlackSmallCastle();
    void UndoWhiteBigCastle();
    void UndoBlackBigCastle();
    void RemoveWhite(byte victim, byte square);
    void AddWhite(byte victim, byte square);
    void MoveWhite(byte piece, byte from, byte to);
    void RemoveBlack(byte victim, byte square);
    void AddBlack(byte victim, byte square);
    void MoveBlack(byte piece, byte from, byte to);
    byte GetWhiteKingPosition();
    byte GetBlackKingPosition();
    int GetPawnValue();
    ulong GetKey();
    PositionsList GetPiecePositions(byte index);
    void GetSquares(byte p, SquareList squares);
    BitBoard GetOccupied();
    BitBoard GetEmpty();
    BitBoard GetBlacks();
    BitBoard GetWhites();
    BitBoard GetPieceBits(byte piece);
    BitBoard GetPerimeter();
    byte UpdatePhase();
    int StaticExchange(AttackBase attack);
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
    bool IsBlackPawnStorm(byte from);
    bool IsWhitePawnStorm(byte from);
    bool IsBlackPawnAttack(byte from);
    bool IsWhitePawnAttack(byte from);
    bool IsBlackCandidate(byte from, byte to);
    bool IsWhiteCandidate(byte from, byte to);
    bool IsBlackRookOnOpenFile(byte from, byte to);
    bool IsWhiteRookOnOpenFile(byte from, byte to);
    bool IsDoubleBlackRook(byte from, byte to);
    bool IsDoubleWhiteRook(byte from, byte to);
    bool IsWhiteRookOnSeven(byte from, byte to);
    bool IsBlackRookOnSeven(byte from, byte to);
    bool IsAttackedByBlackPawn(byte to);
    bool IsAttackedByBlackKnight(byte to);
    bool IsAttackedByBlackBishop(byte to);
    bool IsAttackedByWhiteBishop(byte to);
    bool IsAttackedByWhitePawn(byte to);
    bool IsAttackedByWhiteKnight(byte to);
}