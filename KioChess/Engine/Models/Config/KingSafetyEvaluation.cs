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
    public BlockadeConfiguration BlockadePenalties { get; set; }
    public KingDistanceFactorConfiguration KingDistanceFactor { get; set; }
    public byte NoEscapeSquaresPenalty { get; set; }
    public byte OneEscapeSquarePenalty { get; set; }
    public byte TwoEscapeSquaresPenalty { get; set; }
    public byte ProtectedEscapeSquaresPenalty { get; set; }
}