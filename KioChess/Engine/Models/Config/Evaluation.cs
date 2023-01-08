namespace Engine.Models.Config
{
    public class Evaluation
    {
        #region Implementation of IEvaluation

        public StaticEvaluation Static { get; set; }
        public PieceEvaluation Opening { get; set; }
        public PieceEvaluation Middle { get; set; }
        public PieceEvaluation End { get; set; }

        #endregion
    }
}