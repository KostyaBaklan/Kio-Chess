using Engine.Models.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace StockFishCore.Data
{
    public class ResultContext : DbContext
    {
        public ResultContext()
        {
        }

        public ResultContext(DbContextOptions<ResultContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ResultEntity> ResultEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite("Data Source=C:\\Dev\\ChessDB\\results.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ResultEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("ResultEntity");
            });
        }

        public IEnumerable<StockFishMatchItem> GetMatchItems(DateTime start, DateTime end)
        {
            string query = @"select Depth, StockFishDepth,Elo, Strategy, sum(KioValue) as Kio, sum(sfValue) as SF
                                    from ResultEntity
                                    where time > @start and time < @end
                                    GROUP by Depth, StockFishDepth,Elo, Strategy";

            List<SqliteParameter> parameters = new()
            {
                new SqliteParameter("@start", start),
                new SqliteParameter("@end", end)
            };

            return ExecuteReader(query, r =>
            {
                return new StockFishMatchItem
                {
                    StockFishResultItem = new StockFishResultItem
                    {
                        Depth = r.GetInt16(0),
                        StockFishDepth = r.GetInt16(1),
                        Elo = r.GetInt32(2),
                        Strategy = (StrategyType)Enum.Parse(typeof(StrategyType),r.GetString(3)),
                    },
                    Kio = r.GetDouble(4),
                    SF = r.GetDouble(5),
                };
            }, parameters);
        }

        public IEnumerable<StockFishMatchItem> GetMatchItems()
        {
            string query = @"select Depth, StockFishDepth,Elo, Strategy, sum(KioValue) as Kio, sum(sfValue) as SF
                                    from ResultEntity
                                    GROUP by Depth, StockFishDepth,Elo, Strategy";

            return ExecuteReader(query, r =>
            {
                return new StockFishMatchItem
                {
                    StockFishResultItem = new StockFishResultItem
                    {
                        Depth = r.GetInt16(0),
                        StockFishDepth = r.GetInt16(1),
                        Elo = r.GetInt32(2),
                        Strategy = (StrategyType)r.GetInt32(3),
                    },
                    Kio = r.GetDouble(4),
                    SF = r.GetDouble(5),
                };
            });
        }

        public IEnumerable<T> ExecuteReader<T>(string query, Func<SqliteDataReader, T> itemConverterFunc, 
                                        List<SqliteParameter> parameters = null,
                                        short commandTimeoutSeconds = 30)
        {
            using (var conn = new SqliteConnection(Database.GetConnectionString()))
            {
                conn.Open();

                using var command = conn.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = commandTimeoutSeconds;
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        if (parameter != null)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                }

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return itemConverterFunc(reader);
                }
            }
        }
    }
}