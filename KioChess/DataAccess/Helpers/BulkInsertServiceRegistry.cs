using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Services.BulkInsert;
using System.Collections.Concurrent;

namespace DataAccess.Helpers;

/// <summary>
/// Registry for bulk insert services
/// Maps entity types to their specialized bulk insert implementations
/// </summary>
public static class BulkInsertServiceRegistry
{
    private static readonly ConcurrentDictionary<Type, object> _services = new();

    static BulkInsertServiceRegistry()
    {
        // Register all bulk insert services
        Register<GameEntity>(new GameEntityBulkInsertService());
        Register<PopularPositionEntity>(new PopularPositionEntityBulkInsertService());
    }

    /// <summary>
    /// Register a bulk insert service for an entity type
    /// </summary>
    public static void Register<TEntity>(IBulkInsertService<TEntity> service) where TEntity : class
    {
        _services[typeof(TEntity)] = service;
    }

    /// <summary>
    /// Get the bulk insert service for an entity type
    /// </summary>
    public static IBulkInsertService<TEntity> Get<TEntity>() where TEntity : class
    {
        if (_services.TryGetValue(typeof(TEntity), out var service))
        {
            return (IBulkInsertService<TEntity>)service;
        }

        throw new InvalidOperationException(
            $"No bulk insert service registered for {typeof(TEntity).Name}. " +
            $"Register one using BulkInsertServiceRegistry.Register<{typeof(TEntity).Name}>(service)");
    }

    /// <summary>
    /// Check if a bulk insert service is registered for an entity type
    /// </summary>
    public static bool IsRegistered<TEntity>() where TEntity : class
    {
        return _services.ContainsKey(typeof(TEntity));
    }
}
