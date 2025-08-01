using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface IGeneralConfiguration
{
    int GameDepth { get; }
    int MaxMoveCount { get; }
    double BlockTimeout { get; }
    int FutilityDepth { get; }
    string Strategy { get; }
    HistoryHeuristicConfiguration HistoryHeuristic { get; }
}