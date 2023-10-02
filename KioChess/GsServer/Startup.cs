using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using GamesServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GsServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Enable CoreWCF Services, enable metadata
            // Use the Url used to fetch WSDL as that service endpoint address in generated WSDL 
            services.AddServiceModelServices()
                    .AddServiceModelMetadata()
                    .AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseServiceModel(builder =>
            {
                // Add the Echo Service
                builder.AddService<SequenceService>(serviceOptions =>
                {
                    // Set the default host name:port in generated WSDL and the base path for the address 
                    serviceOptions.BaseAddresses.Add(new Uri($"http://{Config.HOST_IN_WSDL}/{nameof(SequenceService)}"));
                    serviceOptions.BaseAddresses.Add(new Uri($"https://{Config.HOST_IN_WSDL}/{nameof(SequenceService)}"));
                })

                // Add NetTcpBinding
                .AddServiceEndpoint<SequenceService, ISequenceService>(new NetTcpBinding(), $"net.tcp://{Config.HOST_IN_WSDL}:{Config.NETTCP_PORT}/netTcp");
            });
        }
    }

}
