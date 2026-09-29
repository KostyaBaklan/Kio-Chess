namespace Engine.Models.Config;

public class Evaluation
{
    #region Implementation of IEvaluation
    public short[] PieceEvaluation { get; set; }

    public StaticEvaluation Static { get; set; }

    #endregion
}