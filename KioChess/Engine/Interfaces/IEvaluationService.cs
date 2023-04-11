using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IEvaluationService 
    {
        int GetValue(byte piece,  byte phase);
        int GetValue(byte piece, byte square,  byte phase);
        int GetFullValue(byte piece, byte square,  byte phase);
        int GetMateValue();

        int GetUnitValue();
        int GetMinorDefendedByPawnValue( byte phase);
        int GetKnightAttackedByPawnValue( byte phase);

        int GetBlockedPawnValue( byte phase);
        int GetPassedPawnValue( byte phase);
        int GetDoubledPawnValue( byte phase);
        int GetIsolatedPawnValue( byte phase);
        int GetBackwardPawnValue( byte phase);

        int GetNotAbleCastleValue( byte phase);
        int GetEarlyQueenValue( byte phase);
        int GetDoubleBishopValue( byte phase);
        int GetRookOnOpenFileValue( byte phase);
        int GetRentgenValue( byte phase);
        int GetRookConnectionValue( byte phase);
        int GetRookOnHalfOpenFileValue( byte phase);
        int GetBishopBlockedByPawnValue( byte phase);
        int GetRookBlockedByKingValue( byte phase);

        int GetPawnAttackValue();
        int GetKnightAttackValue();
        int GetBishopAttackValue();
        int GetRookAttackValue();
        int GetQueenAttackValue();
        int GetKingAttackValue();
        double GetAttackWeight(int attackCount);
        int GetKingZoneOpenFileValue();
        int GetKingShieldFaceValue();
        int GetKingShieldPreFaceValue();
        int GetOpenPawnValue( byte phase);
        int GetDoubleRookVerticalValue( byte phase);
        int GetDoubleRookHorizontalValue( byte phase);
        int GetBattaryValue( byte phase);

        byte Distance(byte from, byte to);
    }
}