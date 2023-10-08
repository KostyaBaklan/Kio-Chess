using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Contexts;
public class LiteContext : DbContext
{
    public LiteContext()
    {
    }

    public LiteContext(DbContextOptions<LiteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<PositionTotal> Positions { get; set; }

    public virtual DbSet<Opening> Openings { get; set; }

    public virtual DbSet<OpeningSequence> OpeningSequences { get; set; }

    public virtual DbSet<OpeningVariation> OpeningVariations { get; set; }

    public virtual DbSet<Variation> Variations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=C:\\Dev\\ChessDB\\chess.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => new { e.History, e.NextMove });

            entity.HasIndex(e => e.History, "SequenceIndex");
        });

        modelBuilder.Entity<PositionTotal>(entity =>
        {
            entity.HasKey(e => new { e.History, e.NextMove });

            entity.HasIndex(e => e.Total, "TotalIndex");

            entity.ToTable($"{nameof(PositionTotal)}s");
        });

        modelBuilder.Entity<Opening>(entity =>
        {
            entity.HasIndex(e => e.Name, "Openings_Name").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<OpeningSequence>(entity =>
        {
            entity.HasIndex(e => e.Sequence, "OpeningSequences_Sequence");

            entity.Property(e => e.OpeningVariationId).HasColumnName("OpeningVariationID");

            entity.HasOne(d => d.OpeningVariation);
        });

        modelBuilder.Entity<OpeningVariation>(entity =>
        {
            entity.HasIndex(e => e.Name, "OpeningVariations_Name").IsUnique();

            entity.Property(e => e.OpeningId).HasColumnName("OpeningID");
            entity.Property(e => e.VariationId).HasColumnName("VariationID");

            entity.HasOne(d => d.Opening);

            entity.HasOne(d => d.Variation);
        });

        modelBuilder.Entity<Variation>(entity =>
        {
            entity.HasIndex(e => e.Name, "Variations_name").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
        });
    }
}