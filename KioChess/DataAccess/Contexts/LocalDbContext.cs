using DataAccess.Entities;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Contexts
{
    public class LocalDbContext: DbContext
    {
        public virtual DbSet<Debut> Debuts { get; set; }

        public virtual DbSet<PositionTotalDifference> PositionTotalDifferences { get; set; }

        public virtual DbSet<PositionEntity> Positions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite("Data Source=C:\\Dev\\ChessDB\\chessApp.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PositionEntity>(entity =>
            {
                entity.HasKey(e => new { e.Sequence, e.NextMove });

                entity.ToTable($"{nameof(PositionEntity)}");
            });

            modelBuilder.Entity<PositionTotalDifference>(entity =>
            {
                entity.HasKey(e => new { e.Sequence, e.NextMove });

                entity.ToTable($"{nameof(PositionTotalDifference)}");
            });

            modelBuilder.Entity<Debut>(entity =>
            {
                entity.HasKey(e => e.Sequence);

                entity.Property(e => e.Sequence).ValueGeneratedNever();

                entity.ToTable($"{nameof(Debut)}s");
            });
        }
    }
}
