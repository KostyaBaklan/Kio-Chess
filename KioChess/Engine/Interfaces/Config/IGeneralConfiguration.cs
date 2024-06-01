namespace Engine.Interfaces.Config;

public interface IGeneralConfiguration
{
    bool UseEvaluationCache { get; }
    int GameDepth { get; }
    int MaxMoveCount { get; }
    double BlockTimeout { get; }
    bool UseFutility { get; }
    int FutilityDepth { get; }
    bool UseHistory { get; }
    bool UseAging { get; }
    string Strategy { get; }
}