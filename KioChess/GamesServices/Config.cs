using CoreWCF;

namespace GamesServices;

public static class Config
{
    public const int NETTCP_PORT = 8099;
    // Only used on case that UseRequestHeadersForMetadataAddressBehavior is not used
    public const string HOST_IN_WSDL = "127.0.0.1";

    public static NetTcpBinding ServerBinding { get; set; } = new NetTcpBinding
    {
        CloseTimeout = TimeSpan.FromMinutes(1),
        OpenTimeout = TimeSpan.FromMinutes(1),
        SendTimeout = TimeSpan.FromMinutes(600),
        ReceiveTimeout = TimeSpan.FromMinutes(600)
    };

    public static System.ServiceModel.NetTcpBinding ClientBinding { get; set; } = new System.ServiceModel.NetTcpBinding
    {
        CloseTimeout = TimeSpan.FromMinutes(1),
        OpenTimeout = TimeSpan.FromMinutes(1),
        SendTimeout = TimeSpan.FromMinutes(600),
        ReceiveTimeout = TimeSpan.FromMinutes(600)
    };
}
