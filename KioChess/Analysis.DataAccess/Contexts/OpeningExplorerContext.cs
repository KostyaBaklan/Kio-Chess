using Analysis.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analysis.DataAccess.Contexts;

/// <summary>
/// Database context for the Opening Explorer system.
/// Stores chess openings in a navigable tree structure.
/// </summary>
public class OpeningExplorerContext : DbContext
{
    public DbSet<OpeningEntry> Openings { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=C:\\Dev\\ChessDB\\OpeningExplorer.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OpeningEntry>(entity =>
        {
            entity.ToTable("OpeningEntries");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ECO).HasMaxLength(5);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(500);
            entity.Property(e => e.MovesUCI).IsRequired().HasMaxLength(500);
            entity.Property(e => e.MovesSAN).HasMaxLength(500);
            entity.Property(e => e.PositionKey).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FEN).HasMaxLength(200);
            
            // Self-referencing relationship for tree structure
            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes for fast lookups
            entity.HasIndex(e => e.ECO).HasDatabaseName("IX_OpeningEntry_ECO");
            entity.HasIndex(e => e.PositionKey).HasDatabaseName("IX_OpeningEntry_PositionKey");
            entity.HasIndex(e => e.ParentId).HasDatabaseName("IX_OpeningEntry_ParentId");
            entity.HasIndex(e => e.MoveCount).HasDatabaseName("IX_OpeningEntry_MoveCount");
            entity.HasIndex(e => new { e.Popularity, e.IsMainLine })
                  .HasDatabaseName("IX_OpeningEntry_Popularity");
        });
    }
}
