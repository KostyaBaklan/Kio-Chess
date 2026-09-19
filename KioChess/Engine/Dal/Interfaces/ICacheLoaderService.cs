namespace Engine.Dal.Interfaces;

/// <summary>
/// Service interface for loading and building popular move caches
/// </summary>
public interface ICacheLoaderService
{
    /// <summary>
    /// Load popular position caches from kioapp.db for move history
    /// Builds sequence cache and popular move cache for runtime use
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Wait for cache loading to complete
    /// </summary>
    void WaitToData();
}
