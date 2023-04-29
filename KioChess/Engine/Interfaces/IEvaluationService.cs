namespace Engine.Interfaces
{
    public interface IEvaluationService
    {
        short GetValue(byte piece, byte phase);
        short GetValue(byte piece, byte square, byte phase);
        short GetFullValue(byte piece, byte square, byte phase);
        short GetMateValue();

        byte GetUnitValue();
        byte GetMinorDefendedByPawnValue(byte phase);
        byte GetKnightAttackedByPawnValue(byte phase);

        byte GetBlockedPawnValue(byte phase);
        byte GetPassedPawnValue(byte phase);
        byte GetDoubledPawnValue(byte phase);
        byte GetIsolatedPawnValue(byte phase);
        byte GetBackwardPawnValue(byte phase);

        byte GetNotAbleCastleValue(byte phase);
        byte GetEarlyQueenValue(byte phase);
        byte GetDoubleBishopValue(byte phase);
        byte GetRookOnOpenFileValue(byte phase);
        byte GetRentgenValue(byte phase);
        byte GetRookConnectionValue(byte phase);
        byte GetRookOnHalfOpenFileValue(byte phase);
        byte GetBishopBlockedByPawnValue(byte phase);
        byte GetRookBlockedByKingValue(byte phase);

        byte GetPawnAttackValue();
        byte GetKnightAttackValue();
        byte GetBishopAttackValue();
        byte GetRookAttackValue();
        byte GetQueenAttackValue();
        byte GetKingAttackValue();
        double GetAttackWeight(int attackCount);
        byte GetKingZoneOpenFileValue();
        byte GetKingShieldFaceValue();
        byte GetKingShieldPreFaceValue();
        byte GetOpenPawnValue(byte phase);
        byte GetDoubleRookVerticalValue(byte phase);
        byte GetDoubleRookHorizontalValue(byte phase);
        byte GetBattaryValue(byte phase);

        byte Distance(byte from, byte to);
    }
}