using System.Diagnostics;
using System.IO.Pipes;
using Engine.Communication.Common;
using Engine.Communication.Configuration;
using Engine.Communication.Serialization;
using Engine.Communication.Transport;
using Microsoft.Extensions.Logging;

namespace Engine.Communication.Client;

public class NamedPipeServiceClient<TService> : IServiceClient<TService> where TService : class
{
    private readonly ClientConfiguration _configuration;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<NamedPipeServiceClient<TService>> _logger;
    private bool _disposed;

    public NamedPipeServiceClient(
        ClientConfiguration configuration,
        IMessageSerializer serializer,
        ILogger<NamedPipeServiceClient<TService>> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TService GetProxy()
    {
        throw new NotImplementedException("Proxy generation not yet implemented. Use CallAsync with method names.");
    }

    public async Task<TResult> CallAsync<TResult>(string methodName, object parameter = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var request = new PipeRequest<object>
        {
            MethodName = methodName,
            Parameters = parameter
        };

        var attempt = 0;
        Exception lastException = null;

        while (attempt < _configuration.MaxRetries)
        {
            attempt++;

            try
            {
                _logger.LogDebug("Executing request {RequestId} to method {MethodName} (attempt {Attempt}/{MaxRetries})",
                    request.RequestId, methodName, attempt, _configuration.MaxRetries);

                var response = await SendRequestAsync<TResult>(request, cancellationToken).ConfigureAwait(false);

                if (response.Success)
                {
                    _logger.LogDebug("Request {RequestId} completed successfully", request.RequestId);
                    return (TResult)(response.Result ?? default(TResult)!);
                }
                else
                {
                    var errorMsg = $"Service returned error: {response.ErrorMessage} ({response.ErrorType})";
                    _logger.LogError(errorMsg);
                    throw new ServiceException(errorMsg);
                }
            }
            catch (Exception ex) when (ex is not ServiceException && attempt < _configuration.MaxRetries)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Request {RequestId} failed on attempt {Attempt}, retrying...",
                    request.RequestId, attempt);

                await Task.Delay(_configuration.RetryDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        var finalError = $"Failed to execute request after {_configuration.MaxRetries} attempts";
        _logger.LogError(lastException, finalError);
        throw new ServiceConnectionException(finalError, lastException!);
    }

    public async Task CallAsync(string methodName, object parameter = null, CancellationToken cancellationToken = default)
    {
        await CallAsync<object>(methodName, parameter, cancellationToken).ConfigureAwait(false);
    }

    private async Task<PipeResponse<TResult>> SendRequestAsync<TResult>(PipeRequest<object> request, CancellationToken cancellationToken)
    {
        using var pipeClient = new NamedPipeClientStream(
            ".",
            _configuration.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        try
        {
            var connectTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            connectTimeoutCts.CancelAfter(_configuration.ConnectTimeout);

            await pipeClient.ConnectAsync(connectTimeoutCts.Token).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new ServiceTimeoutException($"Connection to service '{typeof(TService).Name}' timed out after {_configuration.ConnectTimeout}");
        }
        catch (IOException)
        {
            if (_configuration.AutoStartServer && !string.IsNullOrEmpty(_configuration.ServerExecutablePath))
            {
                _logger.LogInformation("Server not found, attempting to start: {Path}", _configuration.ServerExecutablePath);
                TryStartServer();
                
                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
                
                await pipeClient.ConnectAsync(_configuration.ConnectTimeout, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new ServiceNotFoundException(typeof(TService).Name);
            }
        }

        var requestData = _serializer.Serialize(request);
        await WriteMessageAsync(pipeClient, requestData, cancellationToken).ConfigureAwait(false);

        var responseData = await ReadMessageAsync(pipeClient, cancellationToken).ConfigureAwait(false);
        var response = _serializer.Deserialize<PipeResponse<TResult>>(responseData);

        return response;
    }

    private void TryStartServer()
    {
        try
        {
            if (!string.IsNullOrEmpty(_configuration.ServerExecutablePath) && File.Exists(_configuration.ServerExecutablePath))
            {
                Process.Start(_configuration.ServerExecutablePath);
                _logger.LogInformation("Started server: {Path}", _configuration.ServerExecutablePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start server: {Path}", _configuration.ServerExecutablePath);
        }
    }

    private async Task<byte[]> ReadMessageAsync(NamedPipeClientStream pipeClient, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[4];
        var totalRead = 0;

        while (totalRead < 4)
        {
            var read = await pipeClient.ReadAsync(lengthBuffer.AsMemory(totalRead, 4 - totalRead), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
            {
                throw new IOException("Pipe closed before length was fully read");
            }
            totalRead += read;
        }

        var length = BitConverter.ToInt32(lengthBuffer, 0);
        var messageBuffer = new byte[length];
        totalRead = 0;

        while (totalRead < length)
        {
            var read = await pipeClient.ReadAsync(messageBuffer.AsMemory(totalRead, length - totalRead), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
            {
                throw new IOException("Pipe closed before message was fully read");
            }
            totalRead += read;
        }

        return messageBuffer;
    }

    private async Task WriteMessageAsync(NamedPipeClientStream pipeClient, byte[] data, CancellationToken cancellationToken)
    {
        var lengthBuffer = BitConverter.GetBytes(data.Length);
        await pipeClient.WriteAsync(lengthBuffer, cancellationToken).ConfigureAwait(false);
        await pipeClient.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        await pipeClient.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }
}
