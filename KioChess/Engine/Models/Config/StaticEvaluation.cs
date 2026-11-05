using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class StaticEvaluation : IStaticEvaluation
{
    #region Implementation of IStaticEvaluation

    public short Mate { get; set; }

    public BoardEvaluation Middle { get; set; }
    public BoardEvaluation End { get; set; }
    public KingSafetyEvaluation KingSafety { get; set; }

    public PassedPawnConfiguration PassedPawnConfiguration { get; set; }

    public PassedPawnConfiguration ProtectedPassedPawnConfiguration { get; set; }

    public PassedPawnConfiguration ConnectedPassedPawnConfiguration { get; set; }

    public DrawConfiguration Draw { get; set; }

    public BoardEvaluation GetBoard(byte phase)
    {
        return phase == Enums.Phase.End ? End : Middle;
    }

    #endregion
}