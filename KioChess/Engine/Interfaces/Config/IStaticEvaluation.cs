using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface IStaticEvaluation
{
    short Mate { get; }

    BoardEvaluation Opening { get; set; }
    BoardEvaluation Middle { get; set; }
    BoardEvaluation End { get; set; }
    KingSafetyEvaluation KingSafety { get; }

    PawnRankConfiguration PassedPawnConfiguration { get; }

    PawnRankConfiguration ProtectedPassedPawnConfiguration { get; }

    PawnRankConfiguration ConnectedPassedPawnConfiguration { get; }

    PawnRankConfiguration CandidatePassedPawnConfiguration { get; }

    BoardEvaluation GetBoard(byte phase);
}