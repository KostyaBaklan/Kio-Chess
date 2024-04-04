namespace Engine.Models.Config;

public class KingSafetyEvaluation
{
    public byte KingShieldFaceValue { get; set; }
    public byte KingShieldPreFaceValue { get; set; }
    public byte KingZoneOpenFileValue { get; set; }
    public byte[] PieceAttackValue { get; set; }
    public byte PawnStormValue4 { get; set; }
    public byte PawnStormValue5 { get; set; }
    public byte PawnStormValue6 { get; set; }
    public int[] AttackWeight { get; set; }
    public byte PawnShield2Value { get; set; }
    public byte PawnShield3Value { get; set; }
    public byte PawnShield4Value { get; set; }
    public byte PawnKingShield2Value { get; set; }
    public byte PawnKingShield3Value { get; set; }
    public byte PawnKingShield4Value { get; set; }
}