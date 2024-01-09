﻿using Engine.DataStructures.Moves.Lists;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Models.Contexts;

namespace Engine.Interfaces;

public interface IPosition
{
    ulong GetKey();
    short GetValue();
    int GetStaticValue();
    int GetKingSafetyValue();
    int GetPawnValue();
    Turn GetTurn();
    bool GetPiece(byte cell, out byte? piece);

    void Make(MoveBase move);
    void UnMake();
    void Do(MoveBase move);
    void UnDo(MoveBase move);
    void SwapTurn();
    List<MoveBase> GetMoves(byte piece, byte to);
    List<MoveBase> GetAllMoves();
    IEnumerable<MoveBase> GetAllMoves(byte cell, byte piece);
    IBoard GetBoard();
    IEnumerable<MoveBase> GetHistory();
    byte GetPhase();
    bool CanWhitePromote();
    bool CanBlackPromote();
    void SaveHistory();
    bool IsDraw();
    void MakeFirst(MoveBase move);
    MoveList GetFirstMoves();
    MoveValueList GetAllBookMoves(SortContext sortContext);
    MoveValueList GetAllMoves(SortContext sortContext);
    MoveValueList GetAllAttacks(SortContext sortContext);
    void GetWhitePromotionAttacks(AttackList attacks);
    void GetWhiteAttacks(AttackList attacks);
    void GetBlackPromotionAttacks(AttackList attacks);
    void GetBlackAttacks(AttackList attacks);
    bool AnyWhiteMoves();
    bool AnyBlackMoves();
    bool IsBlockedByBlack(byte position);
    bool IsBlockedByWhite(byte position);
    void GetWhiteAttacksTo(byte to, AttackList attackList);
    void GetBlackAttacksTo(byte to, AttackList attackList);
    void Clear();
}
