namespace Engine.Models.Config;

public class KingSafetyEvaluation
{
    public byte KingShieldFaceValue { get; set; }
    public byte KingShieldPreFaceValue { get; set; }
    public byte KingZoneOpenFileValue { get; set; }
    public byte[] PieceAttackValue { get; set; }
    public double[] AttackWeight { get; set; }
}