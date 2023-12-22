namespace Engine.Models.Config;

public class BoardEvaluation
{
    public short NotAbleCastleValue { get; set; }
    public short EarlyQueenValue { get; set; }
    public short DoubleBishopValue { get; set; }
    public short MinorDefendedByPawnValue { get; set; }
    public short BlockedPawnValue { get; set; }
    public short PassedPawnValue { get; set; }
    public short ProtectedPassedPawnValue { get; set; }
    public short DoubledPawnValue { get; set; }
    public short IsolatedPawnValue { get; set; }
    public short BackwardPawnValue { get; set; }
    public short RookOnOpenFileValue { get; set; }
    public short RentgenValue { get; set; }
    public int RookOnHalfOpenFileValue { get; set; }
    public int RookBlockedByKingValue { get; set; }
    public int DoubleRookVerticalValue { get; set; }
    public int DoubleRookHorizontalValue { get; set; }
    public int BattaryValue { get; set; }
    public int NoPawnsValue { get; set; }
    public byte ForwardMoveValue { get; set; }
    public byte QueenDistanceToKingValue { get; set; }
    public byte OpenPawnValue { get; set; }
    public byte CandidatePawnValue { get; set; }
    public byte[] MobilityValues { get; set; }
}