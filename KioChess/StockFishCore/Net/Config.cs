using CoreWCF;

namespace StockFishCore
{
    public static class Config
    {
        public const int TIMEOUT = 24 * 60;
        public const int NETTCP_PORT = 8099;
        public const string HOST_IN_WSDL = "127.0.0.1";

        public static NetTcpBinding ServerBinding { get; set; } = new NetTcpBinding
        {
            CloseTimeout = TimeSpan.FromMinutes(TIMEOUT),
            OpenTimeout = TimeSpan.FromMinutes(TIMEOUT),
            SendTimeout = TimeSpan.FromMinutes(TIMEOUT),
            ReceiveTimeout = TimeSpan.FromMinutes(TIMEOUT)
        };

        public static System.ServiceModel.NetTcpBinding ClientBinding { get; set; } = new System.ServiceModel.NetTcpBinding
        {
            CloseTimeout = TimeSpan.FromMinutes(TIMEOUT),
            OpenTimeout = TimeSpan.FromMinutes(TIMEOUT),
            SendTimeout = TimeSpan.FromMinutes(TIMEOUT),
            ReceiveTimeout = TimeSpan.FromMinutes(TIMEOUT)
        };
    }
}