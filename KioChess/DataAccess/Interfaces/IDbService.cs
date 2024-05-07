using Microsoft.Data.Sqlite;

namespace DataAccess.Interfaces;

public interface IDbService
{
    void Connect();
    void Disconnect();
    int Execute(string sql, List<SqliteParameter> parameters = null, int timeout = 30);
    IEnumerable<T> Execute<T>(string sql, Func<SqliteDataReader, T> factoy, List<SqliteParameter> parameters = null, int timeout = 60);
}
