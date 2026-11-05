namespace Engine.Models.Config;

public class BoardEvaluation
{
    public short DoubleBishopValue { get; set; }
    public short BlockedPawnValue { get; set; }
    public short DoubledPawnValue { get; set; }
    public short IsolatedPawnValue { get; set; }
    public short BackwardPawnValue { get; set; }
    public short RookOnOpenFileValue { get; set; }
    public short RentgenValue { get; set; }
    public int RookOnHalfOpenFileValue { get; set; }
    public int RookBlockedByKingValue { get; set; }
    public int NoPawnsValue { get; set; }
    public short[] MobilityValues { get; set; }
    public short RookOnOpenFileNextToKingValue { get; set; }
    public short DoubleRookOnOpenFileValue { get; set; }
    public short RookOnHalfOpenFileNextToKingValue { get; set; }
    public short DoubleRookOnHalfOpenFileValue { get; set; }
    public short ConnectedRooksOnFirstRankValue { get; set; }
    public short DiscoveredCheckValue { get; set; }
    public short DiscoveredAttackValue { get; set; }
    public short AbsolutePinValue { get; set; }
    public short PartialPinValue { get; set; }
    public short BishopBattaryValue { get; set; }
    public short RookBattaryValue { get; set; }
    public short QueenBattaryValue { get; set; }
    public short RookBehindPassedPawnValue { get; set; }
    public short UnstoppablePassedPawnValue { get; set; }
    public short OutsidePassedPawnValue { get; set; }
    public short KnightOutpostValue { get; set; }
    public short BishopOutpostValue { get; set; }
}