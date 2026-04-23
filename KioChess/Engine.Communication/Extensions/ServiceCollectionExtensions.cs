using Engine.Communication.Configuration;
using Engine.Communication.Serialization;
using Engine.Communication.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Engine.Communication.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceHost<TServiceInterface, TServiceImplementation>(
        this IServiceCollection services,
        Action<ServiceConfiguration> configure) 
        where TServiceInterface : class
        where TServiceImplementation : class, TServiceInterface
    {
        var configuration = new ServiceConfiguration();
        configure(configuration);

        services.AddSingleton(configuration);
        services.AddSingleton<IMessageSerializer, MessagePackSerializer>();
        services.AddSingleton<TServiceInterface, TServiceImplementation>();
        services.AddSingleton<IServiceHost>(sp =>
        {
            var config = sp.GetRequiredService<ServiceConfiguration>();
            var serializer = sp.GetRequiredService<IMessageSerializer>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NamedPipeServiceHost<TServiceInterface>>>();
            var serviceInstance = sp.GetRequiredService<TServiceInterface>();
            return new NamedPipeServiceHost<TServiceInterface>(config, serviceInstance, serializer, logger);
        });

        return services;
    }

    public static IServiceCollection AddServiceClient<TService>(
        this IServiceCollection services,
        Action<ClientConfiguration> configure)
        where TService : class
    {
        var configuration = new ClientConfiguration();
        configure(configuration);

        services.AddSingleton(configuration);
        services.AddSingleton<IMessageSerializer, MessagePackSerializer>();
        services.AddSingleton<Client.IServiceClient<TService>, Client.NamedPipeServiceClient<TService>>();

        return services;
    }
}
