﻿using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IMoveProvider
    {
        int MovesCount { get; }
        MoveBase Get(short key);
        IEnumerable<MoveBase> GetAll();
        IEnumerable<AttackBase> GetAttacks(byte piece, byte cell);
        void GetAttacks(byte piece, byte cell, AttackList attackList);
        void GetPromotions(byte piece, byte cell, PromotionList promotions);
        void GetPromotions(byte piece, byte square, List<PromotionAttackList> promotionsAttackTemp);
        void GetMoves(byte piece, byte cell, MoveList moveList);
        IEnumerable<MoveBase> GetMoves(byte piece, byte cell);
        void GetWhiteAttacksToForPromotion(byte to, AttackList attackList);
        void GetBlackAttacksToForPromotion(byte to, AttackList attackList);
        BitBoard GetAttackPattern(byte piece, byte position);
        void SetBoard(IBoard board);
        void AgeHistory();
        void SaveHistory(MoveBase move);

        void GetWhitePawnMoves(SquareList squares, MoveList moveList);
        void GetWhiteKnightMoves(SquareList squares, MoveList moveList);
        void GetWhiteBishopMoves(SquareList squares, MoveList moveList);
        void GetWhiteRookMoves(SquareList squares, MoveList moveList);
        void GetWhiteQueenMoves(SquareList squares, MoveList moveList);
        void GetWhiteKingMoves(SquareList squares, MoveList moveList);
        void GetBlackPawnMoves(SquareList squares, MoveList moveList);
        void GetBlackKnightMoves(SquareList squares, MoveList moveList);
        void GetBlackBishopMoves(SquareList squares, MoveList moveList);
        void GetBlackRookMoves(SquareList squares, MoveList moveList);
        void GetBlackQueenMoves(SquareList squares, MoveList moveList);
        void GetBlackKingMoves(SquareList squares, MoveList moveList);

        void GetWhitePawnMoves(ref BitList squares, MoveList moveList);
        void GetWhiteKnightMoves(ref BitList squares, MoveList moveList);
        void GetWhiteBishopMoves(ref BitList squares, MoveList moveList);
        void GetWhiteRookMoves(ref BitList squares, MoveList moveList);
        void GetWhiteQueenMoves(ref BitList squares, MoveList moveList);
        void GetWhiteKingMoves(ref BitList squares, MoveList moveList);
        void GetBlackPawnMoves(ref BitList squares, MoveList moveList);
        void GetBlackKnightMoves(ref BitList squares, MoveList moveList);
        void GetBlackBishopMoves(ref BitList squares, MoveList moveList);
        void GetBlackRookMoves(ref BitList squares, MoveList moveList);
        void GetBlackQueenMoves(ref BitList squares, MoveList moveList);
        void GetBlackKingMoves(ref BitList squares, MoveList moveList);

        void GetWhitePawnAttacks(SquareList squares, AttackList AttackList);
        void GetWhiteKnightAttacks(SquareList squares, AttackList AttackList);
        void GetWhiteBishopAttacks(SquareList squares, AttackList AttackList);
        void GetWhiteRookAttacks(SquareList squares, AttackList AttackList);
        void GetWhiteQueenAttacks(SquareList squares, AttackList AttackList);
        void GetWhiteKingAttacks(SquareList squares, AttackList AttackList);
        void GetBlackPawnAttacks(SquareList squares, AttackList AttackList);
        void GetBlackKnightAttacks(SquareList squares, AttackList AttackList);
        void GetBlackBishopAttacks(SquareList squares, AttackList AttackList);
        void GetBlackRookAttacks(SquareList squares, AttackList AttackList);
        void GetBlackQueenAttacks(SquareList squares, AttackList AttackList);
        void GetBlackKingAttacks(SquareList squares, AttackList AttackList);
        PromotionList GetWhitePromotions(byte from);
        PromotionList GetBlackPromotions(byte from);
        PromotionAttackList[] GetWhitePromotionAttacks(byte from);
        PromotionAttackList[] GetBlackPromotionAttacks(byte from);
        void GetWhitePawnAttacks(ref BitList squares, AttackList attacks);
        void GetWhiteKnightAttacks(ref BitList squares, AttackList attacks);
        void GetWhiteBishopAttacks(ref BitList squares, AttackList attacks);
        void GetWhiteRookAttacks(ref BitList squares, AttackList attacks);
        void GetWhiteQueenAttacks(ref BitList squares, AttackList attacks);
        void GetWhiteKingAttacks(ref BitList squares, AttackList attacks);
        void GetBlackKingAttacks(ref BitList squares, AttackList attacks);
        void GetBlackQueenAttacks(ref BitList squares, AttackList attacks);
        void GetBlackRookAttacks(ref BitList squares, AttackList attacks);
        void GetBlackBishopAttacks(ref BitList squares, AttackList attacks);
        void GetBlackKnightAttacks(ref BitList squares, AttackList attacks);
        void GetBlackPawnAttacks(ref BitList squares, AttackList attacks);
    }
}