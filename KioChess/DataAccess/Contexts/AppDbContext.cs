using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;

namespace DataAccess.Contexts;

public class AppDbContext : DbContext
{
    public DbSet<ZobristHashKey> ZobristHashKeys { get; set; }
    public DbSet<MoveHash> MoveHashes { get; set; }
    public DbSet<PopularPositionEntity> PopularPositions { get; set; }
    public DbSet<OpeningEntry> OpeningEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Option 1: Use D: drive (original)
            optionsBuilder.UseSqlite("Data Source=C:\\Dev\\ChessDB\\kioapp.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ZobristHashKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Low).HasColumnType("INTEGER");
            entity.Property(e => e.High).HasColumnType("INTEGER");
        });

        modelBuilder.Entity<MoveHash>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Low).HasColumnType("INTEGER");
            entity.Property(e => e.High).HasColumnType("INTEGER");
        });

        modelBuilder.Entity<PopularPositionEntity>(entity =>
        {
            // Composite key: Hash + NextMove
            entity.HasKey(e => new { e.HashLow, e.HashHigh, e.NextMove });

            entity.ToTable("PopularPositions");

            entity.Property(e => e.HashLow).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.HashHigh).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.NextMove).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.Total).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.Length).HasColumnType("INTEGER").IsRequired();

            // Indexes for efficient querying
            // Composite index for the typical filter: WHERE Total > X AND Length < Y
            entity.HasIndex(e => new { e.Total, e.Length })
                .HasDatabaseName("IX_PopularPositions_Total_Length");

            // Index on Hash columns for hash-based lookups
            entity.HasIndex(e => new { e.HashLow, e.HashHigh })
                .HasDatabaseName("IX_PopularPositions_Hash");
        });

        modelBuilder.Entity<OpeningEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("OpeningEntries");

            entity.Property(e => e.Id).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.ECO).HasColumnType("TEXT").HasMaxLength(10);
            entity.Property(e => e.Name).HasColumnType("TEXT").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Variation).HasColumnType("TEXT").HasMaxLength(200);
            entity.Property(e => e.FullName).HasColumnType("TEXT").HasMaxLength(500);
            entity.Property(e => e.MovesUCI).HasColumnType("TEXT").HasMaxLength(500);
            entity.Property(e => e.MovesSAN).HasColumnType("TEXT").HasMaxLength(500);
            entity.Property(e => e.MoveCount).HasColumnType("INTEGER");
            entity.Property(e => e.FEN).HasColumnType("TEXT").HasMaxLength(100);
            entity.Property(e => e.SequenceHashLow).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.SequenceHashHigh).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.ParentId).HasColumnType("INTEGER");
            entity.Property(e => e.Popularity).HasColumnType("INTEGER");
            entity.Property(e => e.IsMainLine).HasColumnType("INTEGER");

            // Indexes for efficient querying
            entity.HasIndex(e => e.ECO).HasDatabaseName("IX_OpeningEntries_ECO");
            entity.HasIndex(e => e.ParentId).HasDatabaseName("IX_OpeningEntries_ParentId");
            entity.HasIndex(e => new { e.SequenceHashLow, e.SequenceHashHigh })
                .HasDatabaseName("IX_OpeningEntries_SequenceHash");
            entity.HasIndex(e => e.MoveCount).HasDatabaseName("IX_OpeningEntries_MoveCount");
        });
    }
}
