namespace Engine.Models.Config;

public class BoardEvaluation
{
    public short NotAbleCastleValue { get; set; }
    public short EarlyQueenValue { get; set; }
    public short DoubleBishopValue { get; set; }
    public short MinorDefendedByPawnValue { get; set; }
    public short BlockedPawnValue { get; set; }
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
    public int NoPawnsValue { get; set; }
    public byte QueenDistanceToKingValue { get; set; }
    public byte OpenPawnValue { get; set; }
    public byte RookOnOpenFileNextToKingValue { get; set; }
    public byte DoubleRookOnOpenFileValue { get; set; }
    public byte RookOnHalfOpenFileNextToKingValue { get; set; }
    public byte DoubleRookOnHalfOpenFileValue { get; set; }
    public byte ConnectedRooksOnFirstRankValue { get; set; }
    public byte DiscoveredCheckValue { get; set; }
    public byte DiscoveredAttackValue { get; set; }
    public byte AbsolutePinValue { get; set; }
    public byte PartialPinValue { get; set; }
    public byte BishopBattaryValue { get; set; }
    public byte RookBattaryValue { get; set; }
    public byte QueenBattaryValue { get; set; }
}