using Engine.DataStructures;
using Engine.Models.Moves;

namespace Engine.Interfaces.Evaluation;

public interface IEvaluationService
{
    short GetPieceValue(byte piece);
    short GetFullValue(byte piece, byte square);
    short GetMateValue();
    byte GetMinorDefendedByPawnValue();
    byte GetKnightAttackedByPawnValue();

    byte GetBlockedPawnValue();
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
    short GetDistance(byte king, byte queen);
    int GetDifference(MoveBase move);
    bool IsForward(MoveBase move); 
    byte GetPawnStormValue4();
    byte GetPawnStormValue5();
    byte GetPawnStormValue6();
    byte GetQueenDistanceToKingValue();
    byte GetOpenPawnValue();
    byte GetProtectedPassedPawnValue();
    byte GetWhitePassedPawnValue(byte coordinate);
    byte GetBlackPassedPawnValue(byte coordinate);
    byte GetWhiteCandidatePawnValue(byte coordinate);
    byte GetBlackCandidatePawnValue(byte coordinate);
    byte GetRookOnBlockedFileValue();
    byte GetRookOnBlockedRankValue();
}