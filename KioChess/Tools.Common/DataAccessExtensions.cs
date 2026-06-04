using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Interfaces.Config;
using System.Diagnostics;

namespace Tools.Common
{
    public static class DataAccessExtensions
    {
        public static  void ProcessPopularPositions(this IAppDbService appDbService, IConfigurationProvider config, IGamesService gamesService)
        {
            Console.WriteLine();
            Console.WriteLine("════════════════════════════════════════════════════════════════════");
            Console.WriteLine("  Updating Popular Positions Cache: games.db → kioapp.db");
            Console.WriteLine("════════════════════════════════════════════════════════════════════");

            var timer = Stopwatch.StartNew();

            try
            {
                Console.WriteLine("Clearing existing popular positions...");
                appDbService.ClearPositions();

                int minGames = config.BookConfiguration.GamesThreshold - 1;
                int maxLength = config.BookConfiguration.SearchDepth + 1;
                int chunkSize = config.BookConfiguration.Chunk;

                Console.WriteLine($"Loading popular positions (min games: {minGames}, max length: {maxLength})...");
                IEnumerable<PopularPositionEntity> positions = gamesService.LoadPopularPositions(minGames, maxLength);

                var chunks = positions.Chunk(chunkSize);

                int totalSize = 0;
                int chunkCount = 0;

                foreach (var chunk in chunks)
                {
                    totalSize += chunk.Length;
                    chunkCount++;
                    Console.WriteLine($"Chunk {chunkCount}: {chunk.Length:N0} positions | Total: {totalSize:N0} | {timer.Elapsed}");

                    appDbService.Add(chunk);
                }

                Console.WriteLine();
                Console.WriteLine($"✓ Total popular positions cached: {appDbService.GetPositionsCount():N0}");
                Console.WriteLine($"✓ Time elapsed: {timer.Elapsed}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating popular positions: {ex.ToFormattedString()}");
                throw;
            }
            finally
            {
                appDbService.Shrink();
                timer.Stop();
            }

        }
    }
}
