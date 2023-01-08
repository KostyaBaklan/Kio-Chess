using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class StaticEvaluation: IStaticEvaluation
    {
        #region Implementation of IStaticEvaluation

        public short Unit { get; set; }
        public short Mate { get; set; }
        public short Factor { get; set; }
        public int ThreefoldRepetitionValue { get; set; }

        public BoardEvaluation Opening { get; set; }
        public BoardEvaluation Middle { get; set; }
        public BoardEvaluation End { get; set; }
        public KingSafetyEvaluation KingSafety { get; set; }

        public BoardEvaluation GetBoard(byte phase)
        {
            if (phase == 0) return Opening;
            if (phase == 1) return Middle;
            return End;
        }

        #endregion
    }
}