using Microsoft.EntityFrameworkCore;

namespace StockFishCore
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
    }
}