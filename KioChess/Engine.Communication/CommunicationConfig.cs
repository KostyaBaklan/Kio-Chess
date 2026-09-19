namespace Engine.Communication;

/// <summary>
/// Common configuration for all communication services in the application.
/// Centralizes pipe names and timeouts for consistency.
/// </summary>
public static class CommunicationConfig
{
    public const int DEFAULT_TIMEOUT_MINUTES = 24 * 60;

    public static class Services
    {
        public static class StockFish
        {
            public const string PipeName = "KioChess.StockFish";
            public const int MaxConcurrentConnections = 100;
            public const int TimeoutMinutes = DEFAULT_TIMEOUT_MINUTES;
        }

        public static class Sequence
        {
            public const string PipeName = "KioChess.Sequence";
            public const int MaxConcurrentConnections = 10;
            public const int TimeoutMinutes = DEFAULT_TIMEOUT_MINUTES;
        }
    }
}
