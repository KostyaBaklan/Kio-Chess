using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class EvaluationProvider : IEvaluationProvider
{
    public EvaluationProvider(StaticEvaluation evaluationStatic, short[] pieceEvaluation)
    {
        Static = evaluationStatic;
        PieceValues = pieceEvaluation;
    }

    #region Implementation of IEvaluationProvider

    public IStaticEvaluation Static { get; }

    public short[] PieceValues { get; }

    #endregion
}