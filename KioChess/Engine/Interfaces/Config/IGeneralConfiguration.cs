using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface IGeneralConfiguration
{
    int DynamicGameDepth { get; }
    int ResizeDepth { get; }
    int ResizeThreshold { get; }
    int MaxMoveCount { get; }
    double BlockTimeout { get; }
    int FutilityDepth { get; }
    string Strategy { get; }
    sbyte[] CutoffDepth { get; }

    int TranspositionTableDepthFactor { get;}

    int TranspositionTableTypeFactor { get;  }
    HistoryHeuristicConfiguration HistoryHeuristic { get; }
}