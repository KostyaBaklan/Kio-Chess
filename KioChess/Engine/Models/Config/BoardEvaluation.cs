namespace Engine.Models.Config
{
    public class BoardEvaluation
    {
        public short NotAbleCastleValue { get; set; }
        public short EarlyQueenValue { get; set; }
        public short DoubleBishopValue { get; set; }
        public short MinorDefendedByPawnValue { get; set; }
        public short BlockedPawnValue { get; set; }
        public short PassedPawnValue { get; set; }
        public short DoubledPawnValue { get; set; }
        public short IsolatedPawnValue { get; set; }
        public short BackwardPawnValue { get; set; }
        public short RookOnOpenFileValue { get; set; }
        public short RentgenValue { get; set; }
        public int RookConnectionValue { get; set; }
        public int RookOnHalfOpenFileValue { get; set; }
        public int KnightAttackedByPawnValue { get; set; }
        public int BishopBlockedByPawnValue { get; set; }
        public int RookBlockedByKingValue { get; set; }
        public int DoubleRookValue { get; set; }
        public int OpenPawnValue { get; set; }
    }
}