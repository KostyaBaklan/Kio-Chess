using DataAccess.Contexts;
using DataAccess.Entities;
using Engine.Dal.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Engine.Dal.Services;

public class MemoryDbService : IMemoryDbService
{
    private readonly SqliteConnection _connection;
    private MemoryContext _context;

    public MemoryDbService()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
    }
    public void Connect()
    {
        _connection.Open();

        var options = new DbContextOptionsBuilder<MemoryContext>()
                    .UseSqlite(_connection) // Set the connection explicitly, so it won't be closed automatically by EF
                    .Options;

        _context = new MemoryContext(options);
        _context.Database.EnsureCreated();
    }

    public void Disconnect()
    {
        try
        {
            _context.Dispose();
            _connection.Close();
        }
        catch (Exception)
        {
        }
    }

    public void Execute(string sql, int timeout = 30)
    {
        _context.Database.ExecuteSqlRaw(sql);
    }

    public IQueryable<T> Execute<T>(string sql)
    {
        return _context.Database.SqlQueryRaw<T>(sql);
    }

    public void Upsert(List<Book> records)
    {
        List<Book> recordsToAdd = new List<Book>();
        List<Book> recordsToUpdate = new List<Book>();

        foreach (var record in records)
        {
            Book temp = _context.Books
                .FirstOrDefault(b => b.History == record.History && b.NextMove == record.NextMove);

            if (temp == null)
            {
                _context.Books.Add(record);
            }
            else
            {
                temp.White += record.White;
                temp.Draw += record.Draw;
                temp.Black += record.Black;
                _context.Books.Update(temp);
            }
        }

        _context.SaveChanges();
    }

    public IQueryable<Book> GetBooks()
    {
        return _context.Books;
    }
}
