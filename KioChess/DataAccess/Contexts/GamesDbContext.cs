using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;

namespace DataAccess.Contexts;

public class GamesDbContext : DbContext
{
    public DbSet<GameEntity> GameEntities { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=C:\\Dev\\ChessDB\\games.db");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameEntity>(entity =>
        {
            // Composite key: Hash + NextMove
            entity.HasKey(e => new { e.Low, e.High, e.NextMove });

            entity.ToTable("GameEntities");

            entity.Property(e => e.Low).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.High).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.NextMove).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.White).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.Black).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.Draw).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.Length).HasColumnType("INTEGER").IsRequired();

            // Indexes for efficient querying
            // Composite index for the typical filter: WHERE Length < Y
            entity.HasIndex(e => new { e.Length })
                .HasDatabaseName("IX_GameEntities_Length");

            // Index on Hash columns for hash-based lookups
            entity.HasIndex(e => new { e.Low, e.High })
                .HasDatabaseName("IX_GameEntities_Hash");
        });
    }
}
