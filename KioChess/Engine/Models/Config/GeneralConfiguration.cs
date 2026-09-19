using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class GeneralConfiguration : IGeneralConfiguration
{
    #region Implementation of IGeneralConfiguration

    public int MaxMoveCount { get; set; }
    public double BlockTimeout { get; set; }
    public int FutilityDepth { get; set; }

    public string Strategy { get; set; }

    public sbyte[] CutoffDepth { get; set; }

    public HistoryHeuristicConfiguration HistoryHeuristic { get; set; }

    public int DynamicGameDepth { get; set; }

    public int ResizeDepth { get; set; }

    public int ResizeThreshold { get; set; }

    public int TranspositionTableDepthFactor { get; set; }

    public int TranspositionTableTypeFactor { get; set; }
    public BoardStateConfiguration BoardState { get; set; }

    #endregion
}