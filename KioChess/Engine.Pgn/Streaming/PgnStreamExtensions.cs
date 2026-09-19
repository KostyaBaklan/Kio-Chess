using Engine.Pgn.Models;
using System.Diagnostics;

namespace Engine.Pgn.Streaming;

/// <summary>
/// Extension methods for filtering and processing PGN game streams efficiently.
/// Optimized for large files with lazy evaluation.
/// </summary>
public static class PgnStreamExtensions
{
    /// <summary>
    /// Filters games by minimum ELO rating (uses minimum of White and Black ELO).
    /// Lazy evaluation - only parses tags, not moves.
    /// </summary>
    public static IEnumerable<PgnGameInfo> WithMinElo(this IEnumerable<PgnGameInfo> games, int minElo)
    {
        foreach (var game in games)
        {
            if (game.MinElo >= minElo)
            {
                yield return game;
            }
        }
    }

    /// <summary>
    /// Filters games where both players have minimum ELO rating.
    /// </summary>
    public static IEnumerable<PgnGameInfo> WithBothPlayersMinElo(this IEnumerable<PgnGameInfo> games, int minElo)
    {
        foreach (var game in games)
        {
            if (game.WhiteElo >= minElo && game.BlackElo >= minElo)
            {
                yield return game;
            }
        }
    }

    /// <summary>
    /// Filters games by custom predicate. Lazy evaluation.
    /// </summary>
    public static IEnumerable<PgnGameInfo> Where(this IEnumerable<PgnGameInfo> games, Func<PgnGameInfo, bool> predicate)
    {
        foreach (var game in games)
        {
            if (predicate(game))
            {
                yield return game;
            }
        }
    }

    /// <summary>
    /// Takes first N games from stream. Stops reading after N games found.
    /// </summary>
    public static IEnumerable<PgnGameInfo> Take(this IEnumerable<PgnGameInfo> games, int count)
    {
        int taken = 0;
        foreach (var game in games)
        {
            if (taken >= count)
                yield break;
            
            yield return game;
            taken++;
        }
    }

    /// <summary>
    /// Counts games matching a condition without storing them in memory.
    /// </summary>
    public static int CountWhere(this IEnumerable<PgnGameInfo> games, Func<PgnGameInfo, bool> predicate)
    {
        int count = 0;
        foreach (var game in games)
        {
            if (predicate(game))
                count++;
        }
        return count;
    }

    /// <summary>
    /// Groups games by ELO ranges and counts them efficiently.
    /// Returns dictionary of minElo -> count of games at or above that ELO.
    /// </summary>
    public static Dictionary<int, int> CountByEloRanges(
        this IEnumerable<PgnGameInfo> games, 
        int startElo, 
        int maxElo, 
        int step = 5)
    {
        var eloRanges = new Dictionary<int, int>();
        
        // Initialize ranges
        for (int elo = startElo; elo <= maxElo; elo += step)
        {
            eloRanges[elo] = 0;
        }

        // Collect all ELOs first
        var elos = new List<int>();
        foreach (var game in games)
        {
            elos.Add(game.MinElo);
        }

        // Count games at each threshold
        foreach (var threshold in eloRanges.Keys.ToList())
        {
            eloRanges[threshold] = elos.Count(e => e >= threshold);
        }

        return eloRanges;
    }

    /// <summary>
    /// Calculates optimal ELO threshold to get desired number of games.
    /// Scans through file once to build ELO distribution.
    /// </summary>
    public static int FindOptimalElo(
        this IEnumerable<PgnGameInfo> games,
        int targetGameCount,
        int startElo = 0,
        int maxElo = 4000,
        int step = 5)
    {
        var eloRanges = games.CountByEloRanges(startElo, maxElo, step);
        
        int bestElo = startElo;
        foreach (var kvp in eloRanges.OrderBy(x => x.Key))
        {
            if (kvp.Value < targetGameCount)
            {
                break;
            }
            bestElo = kvp.Key;
        }

        return bestElo;
    }

    /// <summary>
    /// Reports progress while enumerating games.
    /// Useful for long-running operations on large files.
    /// </summary>
    public static IEnumerable<PgnGameInfo> WithProgress(
        this IEnumerable<PgnGameInfo> games,
        PgnStreamReader reader,
        Action<ProgressInfo> onProgress,
        int reportEveryN = 1000)
    {
        int count = 0;
        var stopwatch = Stopwatch.StartNew();

        foreach (var game in games)
        {
            count++;
            
            if (count % reportEveryN == 0)
            {
                onProgress(new ProgressInfo
                {
                    GamesProcessed = count,
                    ProgressPercent = reader.ProgressPercent,
                    FilePosition = reader.Position,
                    FileLength = reader.Length,
                    Elapsed = stopwatch.Elapsed
                });
            }

            yield return game;
        }

        // Final progress report
        onProgress(new ProgressInfo
        {
            GamesProcessed = count,
            ProgressPercent = 100.0,
            FilePosition = reader.Length,
            FileLength = reader.Length,
            Elapsed = stopwatch.Elapsed
        });
    }

    /// <summary>
    /// Processes games in parallel batches while maintaining streaming.
    /// Good for CPU-intensive operations on each game.
    /// </summary>
    public static IEnumerable<TResult> ProcessBatch<TResult>(
        this IEnumerable<PgnGameInfo> games,
        Func<PgnGameInfo, TResult> processor,
        int batchSize = 100)
    {
        var batch = new List<PgnGameInfo>(batchSize);

        foreach (var game in games)
        {
            batch.Add(game);

            if (batch.Count >= batchSize)
            {
                // Process batch in parallel
                var results = batch.AsParallel().Select(processor).ToList();
                
                foreach (var result in results)
                {
                    yield return result;
                }

                batch.Clear();
            }
        }

        // Process remaining items
        if (batch.Count > 0)
        {
            var results = batch.AsParallel().Select(processor).ToList();
            foreach (var result in results)
            {
                yield return result;
            }
        }
    }
}

public class ProgressInfo
{
    public int GamesProcessed { get; set; }
    public double ProgressPercent { get; set; }
    public long FilePosition { get; set; }
    public long FileLength { get; set; }
    public TimeSpan Elapsed { get; set; }

    public override string ToString()
    {
        return $"Games: {GamesProcessed}, Progress: {ProgressPercent:F2}%, Elapsed: {Elapsed}";
    }
}
