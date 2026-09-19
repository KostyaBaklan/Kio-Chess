namespace Engine.Communication.Server;

public interface IServiceHost : IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    bool IsRunning { get; }
    string ServiceName { get; }
}
