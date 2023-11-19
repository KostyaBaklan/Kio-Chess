﻿using Engine.DataStructures;
using Engine.Models.Moves;

namespace Engine.Interfaces.Evaluation;

public interface IEvaluationService
{
    short GetPieceValue(byte piece);
    short GetFullValue(byte piece, byte square);
    short GetMateValue();

    byte GetUnitValue();
    byte GetMinorDefendedByPawnValue();
    byte GetKnightAttackedByPawnValue();

    byte GetBlockedPawnValue();
    byte GetPassedPawnValue();
    byte GetDoubledPawnValue();
    byte GetIsolatedPawnValue();
    byte GetBackwardPawnValue();
    byte GetDoubleBishopValue();
    byte GetRookOnOpenFileValue();
    byte GetRentgenValue();
    byte GetRookOnHalfOpenFileValue();
    byte GetBishopBlockedByPawnValue();
    byte GetRookBlockedByKingValue();

    byte GetPawnAttackValue();
    byte GetKnightAttackValue();
    byte GetBishopAttackValue();
    byte GetRookAttackValue();
    byte GetQueenAttackValue();
    byte GetKingAttackValue();
    double GetAttackWeight(byte attackCount);
    byte GetKingZoneOpenFileValue();
    byte GetKingShieldFaceValue();
    byte GetKingShieldPreFaceValue();
    short GetNoPawnsValue();
    byte GetDoubleRookVerticalValue();
    byte GetDoubleRookHorizontalValue();
    byte GetBattaryValue();
    short Distance(byte kingPosition, BitList positions);

    bool IsForward(MoveBase move);
}