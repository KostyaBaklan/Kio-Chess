using DataAccess.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Contexts;

public class MemoryContext : DbContext
{
    public MemoryContext()
    {
    }

    public MemoryContext(DbContextOptions<MemoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => new { e.History, e.NextMove });
            entity.Property(e => e.History).IsRequired();
            entity.Property(e => e.NextMove).IsRequired();
            entity.Property(e => e.White).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.Draw).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.Black).IsRequired().HasDefaultValue(0);
            entity.ToTable("Books");
        });
    }

    public void CreateTable()
    {
        var db = Database;
        var connection = db.GetConnectionString();

        using (var cn = new SqliteConnection(connection))
        {
            cn.Open();

            var command  = cn.CreateCommand();

            string sql = "CREATE TABLE \"Books\" (\r\n\t\"History\"\tBLOB NOT NULL,\r\n\t\"NextMove\"\tINTEGER NOT NULL,\r\n\t\"White\"\tINTEGER NOT NULL DEFAULT 0,\r\n\t\"Draw\"\tINTEGER NOT NULL DEFAULT 0,\r\n\t\"Black\"\tINTEGER NOT NULL DEFAULT 0,\r\n\tPRIMARY KEY(\"History\",\"NextMove\")\r\n)";

            command.CommandText = sql;

            command.ExecuteNonQuery();

            command.Dispose();
        }

        db.EnsureCreated();
    }
}
