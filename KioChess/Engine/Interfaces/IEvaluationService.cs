using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IEvaluationService : ICacheService
    {
        int GetValue(byte piece, Phase phase);
        int GetValue(byte piece, byte square, Phase phase);
        int GetFullValue(byte piece, byte square, Phase phase);
        int GetMateValue();
        int Evaluate(IPosition position);
        void Initialize(short depth);

        int GetUnitValue();
        int GetMinorDefendedByPawnValue(Phase phase);
        int GetKnightAttackedByPawnValue(Phase phase);

        int GetBlockedPawnValue(Phase phase);
        int GetPassedPawnValue(Phase phase);
        int GetDoubledPawnValue(Phase phase);
        int GetIsolatedPawnValue(Phase phase);
        int GetBackwardPawnValue(Phase phase);

        int GetNotAbleCastleValue(Phase phase);
        int GetEarlyQueenValue(Phase phase);
        int GetDoubleBishopValue(Phase phase);
        int GetRookOnOpenFileValue(Phase phase);
        int GetRentgenValue(Phase phase);
        int GetRookConnectionValue(Phase phase);
        int GetRookOnHalfOpenFileValue(Phase phase);
        int GetBishopBlockedByPawnValue(Phase phase);
        int GetRookBlockedByKingValue(Phase phase);

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
        int GetOpenPawnValue(Phase phase);
        int GetDoubleRookValue(Phase phase);

        byte Distance(byte from, byte to);
    }
}