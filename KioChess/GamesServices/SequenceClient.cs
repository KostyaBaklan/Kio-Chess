using Engine.Communication;
using Engine.Communication.Client;
using Engine.Communication.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace GamesServices;

public class SequenceClient
{
    private readonly IServiceClient<ISequenceService> _client;

    public SequenceClient()
    {
        var configuration = new ClientConfiguration
        {
            PipeName = CommunicationConfig.Services.Sequence.PipeName,
            ConnectTimeout = TimeSpan.FromSeconds(10),
            OperationTimeout = TimeSpan.FromMinutes(CommunicationConfig.Services.Sequence.TimeoutMinutes),
            MaxRetries = 3,
            RetryDelay = TimeSpan.FromMilliseconds(100)
        };

        var factory = new ServiceClientFactory(NullLoggerFactory.Instance);
        _client = factory.CreateClient<ISequenceService>(configuration);
    }

    public IServiceClient<ISequenceService> GetClient()
    {
        return _client;
    }

    public async Task CloseAsync()
    {
        await _client.DisposeAsync();
    }
}
