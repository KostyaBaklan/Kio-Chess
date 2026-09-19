using System.Diagnostics;
using Engine.Communication;
using Engine.Communication.Client;
using Engine.Communication.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using StockFishCore.Services;

namespace StockFishCore.Net
{
    public class StockFishClient
    {
        private readonly IServiceClient<IStockFishService> _client;

        public StockFishClient()
        {
            var configuration = new ClientConfiguration
            {
                PipeName = CommunicationConfig.Services.StockFish.PipeName,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                OperationTimeout = TimeSpan.FromMinutes(CommunicationConfig.Services.StockFish.TimeoutMinutes),
                MaxRetries = 5,
                RetryDelay = TimeSpan.FromMilliseconds(100),
                AutoStartServer = true,
                ServerExecutablePath = GetServerPath()
            };

            var factory = new ServiceClientFactory(NullLoggerFactory.Instance);
            _client = factory.CreateClient<IStockFishService>(configuration);
        }

        public IServiceClient<IStockFishService> GetClient()
        {
            return _client;
        }

        public async Task CloseAsync()
        {
            await _client.DisposeAsync();
        }

        private static string GetServerPath()
        {
#if DEBUG
            return @$"..\..\..\StockFishServer\bin\Debug\net10.0\StockFishServer.exe";
#else
            return @$"..\..\..\StockFishServer\bin\Release\net10.0\StockFishServer.exe";
#endif
        }

        public static void StartServer()
        {
            var path = GetServerPath();
            if (File.Exists(path))
            {
                Process.Start(path);
            }
        }
    }
}
