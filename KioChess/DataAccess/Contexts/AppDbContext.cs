using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;

namespace DataAccess.Contexts;

public class AppDbContext : DbContext
{
    public DbSet<ZobristHashKey> ZobristHashKeys { get; set; }
    public DbSet<MoveHash> MoveHashes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
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
    }
}
