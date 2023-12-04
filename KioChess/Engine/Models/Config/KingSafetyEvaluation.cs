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
    public double[] AttackWeight { get; set; }
}