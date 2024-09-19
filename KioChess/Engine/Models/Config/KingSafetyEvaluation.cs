namespace Engine.Models.Config;

public class KingSafetyEvaluation
{
    public byte[] PieceAttackValue { get; set; }
    public int[] AttackWeight { get; set; }
    public byte PawnShield2Value { get; set; }
    public byte PawnShield3Value { get; set; }
    public byte PawnShield4Value { get; set; }
    public byte PawnKingShield2Value { get; set; }
    public byte PawnKingShield3Value { get; set; }
    public byte PawnKingShield4Value { get; set; }
    public int TrofismCoefficientValue { get; set; }
}