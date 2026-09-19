namespace Engine.Communication.Configuration;

public class ClientConfiguration
{
    public string PipeName { get; set; } = string.Empty;
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(24);
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public int BufferSize { get; set; } = 65536;
    public bool AutoStartServer { get; set; } = false;
    public string ServerExecutablePath { get; set; }
}
