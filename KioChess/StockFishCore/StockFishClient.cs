
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
            _client = _factory.CreateChannel();
            var channel = _client as IClientChannel;
            channel.Open();
            return _client;
        }

        public void Close()
        {
            var channel = _client as IClientChannel;
            channel.Close();
            _factory.Close();
        }
    }
}