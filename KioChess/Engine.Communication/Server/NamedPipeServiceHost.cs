using System.IO.Pipes;
using Engine.Communication.Configuration;
using Engine.Communication.Serialization;
using Engine.Communication.Transport;
using Microsoft.Extensions.Logging;

namespace Engine.Communication.Server;

public class NamedPipeServiceHost<TService> : IServiceHost where TService : class
{
    private readonly ServiceConfiguration _configuration;
    private readonly TService _serviceInstance;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<NamedPipeServiceHost<TService>> _logger;
    private readonly CancellationTokenSource _shutdownTokenSource;
    private readonly List<Task> _activeTasks;
    private bool _isRunning;

    public bool IsRunning => _isRunning;
    public string ServiceName => typeof(TService).Name;

    public NamedPipeServiceHost(
        ServiceConfiguration configuration,
        TService serviceInstance,
        IMessageSerializer serializer,
        ILogger<NamedPipeServiceHost<TService>> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _serviceInstance = serviceInstance ?? throw new ArgumentNullException(nameof(serviceInstance));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _shutdownTokenSource = new CancellationTokenSource();
        _activeTasks = new List<Task>();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Service {ServiceName} is already running", ServiceName);
            return;
        }

        _logger.LogInformation("Starting service {ServiceName} on pipe {PipeName}", 
            ServiceName, _configuration.PipeName);

        _isRunning = true;

        for (int i = 0; i < _configuration.MaxConcurrentConnections; i++)
        {
            var task = Task.Run(() => ListenForConnectionsAsync(_shutdownTokenSource.Token), cancellationToken);
            _activeTasks.Add(task);
        }

        _logger.LogInformation("Service {ServiceName} started with {MaxConnections} listener threads",
            ServiceName, _configuration.MaxConcurrentConnections);

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            return;
        }

        _logger.LogInformation("Stopping service {ServiceName}", ServiceName);

        _isRunning = false;
        _shutdownTokenSource.Cancel();

        try
        {
            await Task.WhenAll(_activeTasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        _logger.LogInformation("Service {ServiceName} stopped", ServiceName);
    }

    private async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream pipeServer = null;
            try
            {
                pipeServer = new NamedPipeServerStream(
                    _configuration.PipeName,
                    PipeDirection.InOut,
                    _configuration.MaxConcurrentConnections,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    _configuration.BufferSize,
                    _configuration.BufferSize);

                _logger.LogDebug("Waiting for client connection on pipe {PipeName}", _configuration.PipeName);

                await pipeServer.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogDebug("Client connected to pipe {PipeName}", _configuration.PipeName);

                await HandleClientAsync(pipeServer, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in connection listener for {ServiceName}", ServiceName);
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                pipeServer?.Dispose();
            }
        }
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
    {
        try
        {
            var requestData = await ReadMessageAsync(pipeServer, cancellationToken).ConfigureAwait(false);
            
            if (requestData == null || requestData.Length == 0)
            {
                _logger.LogWarning("Received empty request");
                return;
            }

            var request = _serializer.Deserialize<PipeRequest<object>>(requestData);
            
            _logger.LogDebug("Processing request {RequestId} for method {MethodName}", 
                request.RequestId, request.MethodName);

            var response = await ProcessRequestAsync(request, cancellationToken).ConfigureAwait(false);

            var responseData = _serializer.Serialize(response);
            await WriteMessageAsync(pipeServer, responseData, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Completed request {RequestId}", request.RequestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client request");
            
            try
            {
                var errorResponse = new PipeResponse<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name
                };
                var errorData = _serializer.Serialize(errorResponse);
                await WriteMessageAsync(pipeServer, errorData, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }

    private async Task<PipeResponse<object>> ProcessRequestAsync(PipeRequest<object> request, CancellationToken cancellationToken)
    {
        try
        {
            var method = typeof(TService).GetMethod(request.MethodName);
            if (method == null)
            {
                return new PipeResponse<object>
                {
                    RequestId = request.RequestId,
                    Success = false,
                    ErrorMessage = $"Method '{request.MethodName}' not found on service {ServiceName}",
                    ErrorType = "MethodNotFoundException"
                };
            }

            var parameters = method.GetParameters();
            object[] args = null;

            if (parameters.Length > 0 && request.Parameters != null)
            {
                args = new object[] { request.Parameters };
            }
            else
            {
                args = Array.Empty<object>();
            }

            var result = method.Invoke(_serviceInstance, args);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                
                var resultProperty = task.GetType().GetProperty("Result");
                result = resultProperty?.GetValue(task);
            }

            return new PipeResponse<object>
            {
                RequestId = request.RequestId,
                Success = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request {RequestId} for method {MethodName}",
                request.RequestId, request.MethodName);

            return new PipeResponse<object>
            {
                RequestId = request.RequestId,
                Success = false,
                ErrorMessage = ex.InnerException?.Message ?? ex.Message,
                ErrorType = ex.InnerException?.GetType().Name ?? ex.GetType().Name
            };
        }
    }

    private async Task<byte[]> ReadMessageAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[4];
        await pipeServer.ReadAsync(lengthBuffer, 0, 4, cancellationToken).ConfigureAwait(false);
        var length = BitConverter.ToInt32(lengthBuffer, 0);

        var messageBuffer = new byte[length];
        var totalRead = 0;

        while (totalRead < length)
        {
            var read = await pipeServer.ReadAsync(messageBuffer, totalRead, length - totalRead, cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
            {
                throw new IOException("Pipe closed before message was fully read");
            }
            totalRead += read;
        }

        return messageBuffer;
    }

    private async Task WriteMessageAsync(NamedPipeServerStream pipeServer, byte[] data, CancellationToken cancellationToken)
    {
        var lengthBuffer = BitConverter.GetBytes(data.Length);
        await pipeServer.WriteAsync(lengthBuffer, 0, 4, cancellationToken).ConfigureAwait(false);
        await pipeServer.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        await pipeServer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _shutdownTokenSource.Dispose();
        
        if (_serviceInstance is IDisposable disposable)
        {
            disposable.Dispose();
        }
        else if (_serviceInstance is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
    }
}
