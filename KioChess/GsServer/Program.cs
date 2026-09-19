using Engine.Communication;
using Engine.Communication.Extensions;
using GamesServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddServiceHost<ISequenceService, SequenceService>(options =>
{
    options.PipeName = CommunicationConfig.Services.Sequence.PipeName;
    options.MaxConcurrentConnections = CommunicationConfig.Services.Sequence.MaxConcurrentConnections;
    options.Timeout = TimeSpan.FromMinutes(CommunicationConfig.Services.Sequence.TimeoutMinutes);
    options.ServiceDisplayName = "Game Sequence Processing Service";
});

var host = builder.Build();

var serviceHost = host.Services.GetRequiredService<Engine.Communication.Server.IServiceHost>();

Console.WriteLine($"Starting Sequence Service on pipe: {CommunicationConfig.Services.Sequence.PipeName}");
await serviceHost.StartAsync();

Console.WriteLine("Sequence Service is running. Press Ctrl+C to stop.");

await host.WaitForShutdownAsync();

await serviceHost.StopAsync();
Console.WriteLine("Sequence Service stopped.");