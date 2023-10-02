using System.ServiceModel;

namespace GamesServices
{
    public class SequenceClient
    {
        private readonly ChannelFactory<ISequenceService> _factory;
        private ISequenceService _client;

        public SequenceClient() 
        {
            _factory = new ChannelFactory<ISequenceService>(new NetTcpBinding(), new EndpointAddress($"net.tcp://{Config.HOST_IN_WSDL}:{Config.NETTCP_PORT}/netTcp"));
            _factory.Open();
        }

        public ISequenceService GetService()
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
