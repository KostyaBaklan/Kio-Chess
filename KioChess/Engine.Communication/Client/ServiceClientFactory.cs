using Engine.Communication.Configuration;
using Engine.Communication.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Engine.Communication.Client;

public class ServiceClientFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMessageSerializer _serializer;

    public ServiceClientFactory(ILoggerFactory loggerFactory = null, IMessageSerializer serializer = null)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _serializer = serializer ?? new MessagePackSerializer();
    }

    public IServiceClient<TService> CreateClient<TService>(ClientConfiguration configuration) where TService : class
    {
        var logger = _loggerFactory.CreateLogger<NamedPipeServiceClient<TService>>();
        return new NamedPipeServiceClient<TService>(configuration, _serializer, logger);
    }

    public IServiceClient<TService> CreateClient<TService>(Action<ClientConfiguration> configure) where TService : class
    {
        var configuration = new ClientConfiguration();
        configure(configuration);
        return CreateClient<TService>(configuration);
    }
}
