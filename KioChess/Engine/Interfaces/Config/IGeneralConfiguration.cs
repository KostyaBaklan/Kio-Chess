using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface IGeneralConfiguration
{
    int DynamicGameDepth { get; }
    int ResizeDepth { get; }
    int ResizeThreshold { get; }
    int MaxMoveCount { get; }
    int PawnHashBits { get; }
    int EvaluationCacheBits { get; }
    string Strategy { get; }

    TranspositionTableConfiguration TranspositionTable { get; }
    HistoryHeuristicConfiguration HistoryHeuristic { get; }
    BoardStateConfiguration BoardState { get; }
}