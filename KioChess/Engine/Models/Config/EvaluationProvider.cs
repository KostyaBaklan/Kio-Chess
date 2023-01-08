using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class EvaluationProvider: IEvaluationProvider
    {
        private readonly IPieceEvaluation[] _piece;
        public EvaluationProvider(StaticEvaluation evaluationStatic, IPieceEvaluation evaluationOpening, IPieceEvaluation evaluationMiddle, IPieceEvaluation evaluationEnd)
        {
            Static = evaluationStatic;
            _piece = new[] {evaluationOpening, evaluationMiddle, evaluationEnd};
        }

        #region Implementation of IEvaluationProvider

        public IStaticEvaluation Static { get; }

        public IStaticEvaluation GetStatic(byte phase)
        {
            throw new System.NotImplementedException();
        }

        public IPieceEvaluation GetPiece(byte phase)
        {
            return _piece[phase];
        }

        #endregion
    }
}