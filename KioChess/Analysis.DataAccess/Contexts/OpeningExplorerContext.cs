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
            optionsBuilder.UseSqlite("Data Source=C:\\Dev\\ChessDB\\chessApp.db");
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
            entity.Property(e => e.FEN).HasMaxLength(200);

            // Configure MoveKeys as binary data (BLOB)
            entity.Property(e => e.MoveKeys)
                .HasConversion(
                    v => v == null || v.Length == 0 
                        ? null 
                        : ConvertMoveKeysToBytes(v),
                    v => v == null || v.Length == 0 
                        ? new short[0]
                        : ConvertBytesToMoveKeys(v))
                .HasColumnType("BLOB");

            // Configure SequenceHash as unsigned long integer
            entity.Property(e => e.SequenceHash)
                .HasColumnType("INTEGER");

            // Self-referencing relationship for tree structure
            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes for fast lookups
            entity.HasIndex(e => e.ECO).HasDatabaseName("IX_OpeningEntry_ECO");
            entity.HasIndex(e => e.SequenceHash).HasDatabaseName("IX_OpeningEntry_SequenceHash");
            entity.HasIndex(e => e.ParentId).HasDatabaseName("IX_OpeningEntry_ParentId");
            entity.HasIndex(e => e.MoveCount).HasDatabaseName("IX_OpeningEntry_MoveCount");
            entity.HasIndex(e => new { e.Popularity, e.IsMainLine })
                  .HasDatabaseName("IX_OpeningEntry_Popularity");
        });
    }

    /// <summary>
    /// Convert short[] move keys to byte[] for database storage (BLOB).
    /// </summary>
    private static byte[] ConvertMoveKeysToBytes(short[] moveKeys)
    {
        if (moveKeys == null || moveKeys.Length == 0)
            return new byte[0];

        byte[] bytes = new byte[moveKeys.Length * 2];
        Buffer.BlockCopy(moveKeys, 0, bytes, 0, moveKeys.Length * 2);
        return bytes;
    }

    /// <summary>
    /// Convert byte[] from database (BLOB) back to short[] move keys.
    /// </summary>
    private static short[] ConvertBytesToMoveKeys(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return new short[0];

        short[] moveKeys = new short[bytes.Length / 2];
        Buffer.BlockCopy(bytes, 0, moveKeys, 0, bytes.Length);
        return moveKeys;
    }
}

