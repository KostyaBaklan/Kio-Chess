namespace Engine.Communication.Configuration;

public class ServiceConfiguration
{
    public string PipeName { get; set; } = string.Empty;
    public int MaxConcurrentConnections { get; set; } = 10;
    public int BufferSize { get; set; } = 65536;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(24);
    public bool AutoStart { get; set; } = true;
    public string ServiceDisplayName { get; set; }
}
