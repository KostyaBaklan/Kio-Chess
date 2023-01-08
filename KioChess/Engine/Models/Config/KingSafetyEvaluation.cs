namespace Engine.Models.Config
{
    public class KingSafetyEvaluation
    {
        public int KingShieldFaceValue { get; set; }
        public int KingShieldPreFaceValue { get; set; }
        public int KingZoneOpenFileValue { get; set; }
        public double AttackValueFactor { get; set; }
        public int[] PieceAttackValue { get; set; }
        public double[] AttackWeight { get; set; }
    }
}