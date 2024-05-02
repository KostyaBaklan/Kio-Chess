using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StockFishCore;
using System.Reflection;

internal class Program
{
    public static void Main(string[] args)
    {
        IWebHost host = CreateWebHostBuilder(args).Build();
        host.Run();
    }

    // Listen on 8088 for http, and 8443 for https, 8089 for NetTcp.
    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        CreateBuilder(args)
        .UseNetTcp(Config.NETTCP_PORT)
        .UseStartup<Startup>();

    public static IWebHostBuilder CreateBuilder(string[] args)
    {
        WebHostBuilder webHostBuilder = new WebHostBuilder();
        if (string.IsNullOrEmpty(webHostBuilder.GetSetting(WebHostDefaults.ContentRootKey)))
        {
            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
        }

        if (args != null)
        {
            webHostBuilder.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build());
        }

        webHostBuilder.UseKestrel(delegate (WebHostBuilderContext builderContext, KestrelServerOptions options)
        {
            options.Configure(builderContext.Configuration.GetSection("Kestrel"));
        }).ConfigureAppConfiguration(delegate (WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            IHostingEnvironment hostingEnvironment = hostingContext.HostingEnvironment;
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddJsonFile("appsettings." + hostingEnvironment.EnvironmentName + ".json", optional: true, reloadOnChange: true);
            if (hostingEnvironment.IsDevelopment())
            {
                Assembly assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                if (assembly != null)
                {
                    config.AddUserSecrets(assembly, optional: true);
                }
            }

            config.AddEnvironmentVariables();
            if (args != null)
            {
                config.AddCommandLine(args);
            }
        })
            .ConfigureServices(delegate (WebHostBuilderContext hostingContext, IServiceCollection services)
            {
                services.PostConfigure(delegate (HostFilteringOptions options)
                {
                    if (options.AllowedHosts == null || options.AllowedHosts.Count == 0)
                    {
                        string[] array = hostingContext.Configuration["AllowedHosts"]?.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        options.AllowedHosts = (array != null && array.Length != 0) ? array : new string[1] { "*" };
                    }
                });
                services.AddSingleton((IOptionsChangeTokenSource<HostFilteringOptions>)new ConfigurationChangeTokenSource<HostFilteringOptions>(hostingContext.Configuration));
                //services.AddTransient<IStartupFilter, HostFilteringStartupFilter>();
            })
            .UseIIS()
            .UseIISIntegration()
            .UseDefaultServiceProvider(delegate (WebHostBuilderContext context, ServiceProviderOptions options)
            {
                options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            });
        return webHostBuilder;
    }
}