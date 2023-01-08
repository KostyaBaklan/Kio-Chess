namespace Engine.Interfaces.Config
{
    public interface IEvaluationProvider
    {
        IStaticEvaluation Static { get; }
        IPieceEvaluation GetPiece(byte phase);
    }
}