namespace Engine.Communication.Client;

public interface IServiceClient<TService> : IAsyncDisposable where TService : class
{
    Task<TResult> CallAsync<TResult>(string methodName, object parameter = null, CancellationToken cancellationToken = default);
    Task CallAsync(string methodName, object parameter = null, CancellationToken cancellationToken = default);
    TService GetProxy();
}

public interface IServiceProxy
{
    Task<TResult> InvokeAsync<TResult>(string methodName, object parameter = null);
    Task InvokeAsync(string methodName, object parameter = null);
}
