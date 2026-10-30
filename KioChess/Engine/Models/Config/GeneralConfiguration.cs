using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class GeneralConfiguration : IGeneralConfiguration
{
    #region Implementation of IGeneralConfiguration

    public int MaxMoveCount { get; set; }

    public int PawnHashBits { get; set; }

    public int EvaluationCacheBits { get; set; }

    public string Strategy { get; set; }

    public HistoryHeuristicConfiguration HistoryHeuristic { get; set; }
    public int DynamicGameDepth { get; set; }

    public int ResizeDepth { get; set; }

    public int ResizeThreshold { get; set; }

    public TranspositionTableConfiguration TranspositionTable { get; set; }

    public BoardStateConfiguration BoardState { get; set; }

    #endregion
}