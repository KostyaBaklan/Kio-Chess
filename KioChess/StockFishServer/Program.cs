using Engine.Communication;
using Engine.Communication.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockFishCore.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddServiceHost<IStockFishService, StockFishService>(options =>
{
    options.PipeName = CommunicationConfig.Services.StockFish.PipeName;
    options.MaxConcurrentConnections = CommunicationConfig.Services.StockFish.MaxConcurrentConnections;
    options.Timeout = TimeSpan.FromMinutes(CommunicationConfig.Services.StockFish.TimeoutMinutes);
    options.ServiceDisplayName = "StockFish Chess Engine Service";
});

var host = builder.Build();

var serviceHost = host.Services.GetRequiredService<Engine.Communication.Server.IServiceHost>();

Console.WriteLine($"Starting StockFish Service on pipe: {CommunicationConfig.Services.StockFish.PipeName}");
await serviceHost.StartAsync();

Console.WriteLine("StockFish Service is running. Press Ctrl+C to stop.");

await host.WaitForShutdownAsync();

await serviceHost.StopAsync();
Console.WriteLine("StockFish Service stopped.");