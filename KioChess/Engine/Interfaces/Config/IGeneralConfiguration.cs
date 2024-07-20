namespace Engine.Interfaces.Config;

public interface IGeneralConfiguration
{
    int GameDepth { get; }
    int MaxMoveCount { get; }
    double BlockTimeout { get; }
    int FutilityDepth { get; }
    string Strategy { get; }
}