namespace Engine.Interfaces.Config;

public interface IEvaluationProvider
{
    IStaticEvaluation Static { get; }
    short[] PieceValues { get; }
}