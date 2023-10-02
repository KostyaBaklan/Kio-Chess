using CoreWCF.Configuration;
using GamesServices;
using GsServer;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

internal class Program
{
    public static void Main(string[] args)
    {
        IWebHost host = CreateWebHostBuilder(args).Build();
        host.Run();
    }

    // Listen on 8088 for http, and 8443 for https, 8089 for NetTcp.
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
        .UseNetTcp(Config.NETTCP_PORT)
        .UseStartup<Startup>();
}