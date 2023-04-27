namespace Engine.Interfaces
{
    public interface IEvaluationService
    {
        short GetValue(byte piece, byte phase);
        short GetValue(byte piece, byte square, byte phase);
        short GetFullValue(byte piece, byte square, byte phase);
        short GetMateValue();

        short GetUnitValue();
        short GetMinorDefendedByPawnValue(byte phase);
        short GetKnightAttackedByPawnValue(byte phase);

        short GetBlockedPawnValue(byte phase);
        short GetPassedPawnValue(byte phase);
        short GetDoubledPawnValue(byte phase);
        short GetIsolatedPawnValue(byte phase);
        short GetBackwardPawnValue(byte phase);

        short GetNotAbleCastleValue(byte phase);
        short GetEarlyQueenValue(byte phase);
        short GetDoubleBishopValue(byte phase);
        short GetRookOnOpenFileValue(byte phase);
        short GetRentgenValue(byte phase);
        short GetRookConnectionValue(byte phase);
        short GetRookOnHalfOpenFileValue(byte phase);
        short GetBishopBlockedByPawnValue(byte phase);
        short GetRookBlockedByKingValue(byte phase);

        short GetPawnAttackValue();
        short GetKnightAttackValue();
        short GetBishopAttackValue();
        short GetRookAttackValue();
        short GetQueenAttackValue();
        short GetKingAttackValue();
        double GetAttackWeight(int attackCount);
        short GetKingZoneOpenFileValue();
        short GetKingShieldFaceValue();
        short GetKingShieldPreFaceValue();
        short GetOpenPawnValue(byte phase);
        short GetDoubleRookVerticalValue(byte phase);
        short GetDoubleRookHorizontalValue(byte phase);
        short GetBattaryValue(byte phase);

        byte Distance(byte from, byte to);
    }
}