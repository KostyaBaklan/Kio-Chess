namespace Engine.Interfaces.Config
{
    public interface IGeneralConfiguration
    {
        bool UseEvaluationCache { get; }
        int GameDepth { get; }
        double BlockTimeout { get; }
        bool UseFutility { get; }
        int FutilityDepth { get; }
        bool UseHistory { get; }
        int KillerCapacity { get; }
        bool UseAging { get; }
    }
}