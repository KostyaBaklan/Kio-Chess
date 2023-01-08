using Engine.Models.Config;

namespace Engine.Interfaces.Config
{
    public interface IStaticEvaluation
    {
        short Unit { get; }
        short Mate { get; }
        short Factor { get; }
        int ThreefoldRepetitionValue { get; }

        BoardEvaluation Opening { get; set; }
        BoardEvaluation Middle { get; set; }
        BoardEvaluation End { get; set; }
        KingSafetyEvaluation KingSafety { get; }

        BoardEvaluation GetBoard(byte phase);
    }
}