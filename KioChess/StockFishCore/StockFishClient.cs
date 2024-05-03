
using System.Diagnostics;
using System.ServiceModel;

namespace StockFishCore
{
    public class StockFishClient
    {
        private readonly ChannelFactory<IStockFishService> _factory;
        private IStockFishService _client;

        public StockFishClient()
        {
            _factory = new ChannelFactory<IStockFishService>(Config.ClientBinding, new EndpointAddress($"net.tcp://{Config.HOST_IN_WSDL}:{Config.NETTCP_PORT}/netTcp"));
            _factory.Open();
        }

        public IStockFishService GetService()
        {
            try
            {
                return OpenChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to create channel {e}");

                return RetryOpenChannel();
            }
        }

        private IStockFishService RetryOpenChannel()
        {
            Console.WriteLine($"Try to re-open the communication");

            for (int i = 0; i < 5; i++)
            {
                if (!IsServerIsRunning())
                {
                    StartServer();
                }
                try
                {
                    return OpenChannel();
                }
                catch (Exception)
                {
                    Thread.Sleep(10);
                }
            }

            throw new ApplicationException("Unable to run Stockfish server");
        }

        private bool IsServerIsRunning()
        {
            //Debugger.Launch();
            var ps = Process.GetProcessesByName("StockFishServer");

            return ps.Length > 0;
        }

        private IStockFishService OpenChannel()
        {
            _client = _factory.CreateChannel();
            var channel = _client as IClientChannel;
            channel.Open();
            return _client;
        }

        public void Close()
        {
            var channel = _client as IClientChannel;
            channel?.Close();
            _factory?.Close();
        }

        public static void StartServer()
        {
#if DEBUG
            Process.Start(@$"..\..\..\StockFishServer\bin\Debug\net7.0\StockFishServer.exe");
#else
            Process.Start(@$"..\..\..\StockFishServer\bin\Release\net7.0\StockFishServer.exe");
# endif
        }
    }
}