using DataAccess.Interfaces;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services
{
    public class BulkDbService : LiteDbServiceBase, IBulkDbService
    {
        public BulkDbService()
        {
            _connection = new SqliteConnection("Data Source=C:\\Dev\\ChessDB\\chess.db");
        }
        public override void Connect()
        {
            _connection.Open();
        }
    }
}
