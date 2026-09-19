using DataAccess.Helpers;
using DataAccess.Interfaces;
using DataAccess.Models;
using Engine.Dal.Interfaces;
using Engine.Dal.Models;
using Engine.DataStructures.Moves;
using Engine.Interfaces.Config;
using Engine.Services;

namespace Engine.Dal.Services;

/// <summary>
/// Service for loading and building popular move caches from kioapp.db
/// Centralizes cache-building logic previously in GamesService.LoadAsync()
/// </summary>
public class CacheLoaderService : ICacheLoaderService
{
    private readonly short _games;
    private readonly int _search;
    private readonly int _popular;
    private readonly int _popularDepth;
    private readonly int _minimumPopular;
    private readonly int _minimumPopularThreshold;
    private readonly int _maximumPopularThreshold;

    private Task _loadTask;
    private readonly MoveHistoryService _moveHistory;
    private readonly IAppDbService _appDbService;

    public CacheLoaderService(
        IConfigurationProvider configurationProvider,
        MoveHistoryService moveHistory,
        IAppDbService appDbService)
    {
        _games = configurationProvider.BookConfiguration.GamesThreshold;
        _search = configurationProvider.BookConfiguration.SearchDepth;
        _popular = configurationProvider.BookConfiguration.PopularThreshold;
        _popularDepth = configurationProvider.BookConfiguration.PopularDepth;
        _minimumPopular = configurationProvider.BookConfiguration.MinimumPopular;
        _minimumPopularThreshold = configurationProvider.BookConfiguration.MinimumPopularThreshold;
        _maximumPopularThreshold = configurationProvider.BookConfiguration.MaximumPopularThreshold;
        _moveHistory = moveHistory;
        _appDbService = appDbService;
    }

    /// <summary>
    /// Load popular position caches from kioapp.db for move history
    /// Builds sequence cache and popular move cache for runtime use
    /// </summary>
    public Task LoadAsync()
    {
        _loadTask = Task.Factory.StartNew(() =>
        {
            var positions = _appDbService.GetPopularPositions(_games - 1, _search + 1);

            // 128-bit hash version
            var groups = positions.GroupBy(
                p => p.Hash,
                g => new BookMove
                {
                    Id = g.NextMove,
                    Value = g.Total
                });

            Dictionary<UInt128, PopularMoves> map = new(positions.Count);

            foreach (var item in groups)
            {
                map[item.Key] = GetMaxItems(item);
            }

            _moveHistory.CreateSequenceCache(map);

            // 128-bit popular cache
            Dictionary<UInt128, MoveHistory[]> popularMap = new(10000);

            var popularGroups = positions.Where(p => p.Length <= _popularDepth && p.Total >= _minimumPopular)
                .GroupBy(
                    p => p.Hash,
                    g => new MoveHistory(g.NextMove, g.Total))
                .Where(g => g.Count() >= _minimumPopularThreshold);

            foreach (var gr in popularGroups)
            {
                var item = gr.OrderByDescending(x => x.History);

                if (gr.Key != UInt128.Zero)
                {
                    popularMap[gr.Key] = [.. item.Take(_maximumPopularThreshold)];
                }
                else
                {
                    var data = item.Take(_maximumPopularThreshold).ToArray();
                    data.Shuffle();
                    popularMap[gr.Key] = data;
                }
            }

            _moveHistory.CreatePopularCache(popularMap);
        });

        return _loadTask;
    }

    /// <summary>
    /// Wait for cache loading to complete
    /// </summary>
    public void WaitToData()
    {
        if (_loadTask != null && !_loadTask.IsCompleted)
        {
            _loadTask.Wait();
        }
    }

    private PopularMoves GetMaxItems(IGrouping<UInt128, BookMove> item)
    {
        var moves = item
            .OrderByDescending(x => x.Value)
            .Take(_popular)
            .ToArray();

        return moves.Length > 0 ? new Popular(moves) : PopularMoves.Default;
    }
}
