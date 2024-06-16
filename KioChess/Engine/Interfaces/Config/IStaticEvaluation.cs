using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface IStaticEvaluation
{
    short Mate { get; }

    BoardEvaluation Opening { get; set; }
    BoardEvaluation Middle { get; set; }
    BoardEvaluation End { get; set; }
    KingSafetyEvaluation KingSafety { get; }

    PassedPawnConfiguration PassedPawnConfiguration { get; }

    MobilityConfiguration MobilityConfiguration { get; }

    BoardEvaluation GetBoard(byte phase);
}