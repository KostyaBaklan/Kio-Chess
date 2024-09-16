using DataAccess.Contexts;
using DataAccess.Entities;
using DataAccess.Helpers;
using DataAccess.Models;
using Engine.Dal.Interfaces;
using Engine.Interfaces.Config;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Engine.Dal.Services
{
    public class LocalDbService : ILocalDbService
    {
        private readonly int _search;
        private int _games;

        private LocalDbContext Connection;
        public LocalDbService(IConfigurationProvider configurationProvider)
        {
            _search = configurationProvider.BookConfiguration.SearchDepth + 1;
            _games = configurationProvider.BookConfiguration.GamesThreshold - 1;
        }

        public void Connect() => Connection = new LocalDbContext();
        public void Disconnect() => Connection.Dispose();

        public int Execute(string sql, List<SqliteParameter> parameters = null, int timeout = 30)
        {
            using var connction = new SqliteConnection(Connection.Database.GetConnectionString());
            return connction.Execute(sql, parameters, timeout);
        }

        public IEnumerable<T> Execute<T>(string sql, Func<SqliteDataReader, T> factory, List<SqliteParameter> parameters = null, int timeout = 60)
        {
            using var connction = new SqliteConnection(Connection.Database.GetConnectionString());
            return connction.Execute(sql, factory, parameters, timeout);
        }

        public void Add(IEnumerable<PositionTotalDifference> records)
        {
            using (var connection = new SqliteConnection(Connection.Database.GetConnectionString()))
            {
                connection.Open();
                connection.Insert(records);
            }
        }

        public void ClearPositionTotalDifference()
        {
            Connection.PositionTotalDifferences.ExecuteDelete();
            Connection.SaveChanges();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<PositionTotalDifference> GetPositionTotalDifference() => Connection.PositionTotalDifferences.AsNoTracking();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<PositionTotalDifference> GetPositionTotalDifferenceList()
        {
            //var query = Connection.PositionTotalDifferences.AsNoTracking()
            //    .Where(ptd => ptd.Total > _games && ptd.Sequence.Length < _search);

            var query = Connection.PositionTotalDifferences.AsNoTracking();

            List<PositionTotalDifference> positions = new List<PositionTotalDifference>(2300000);
            positions.AddRange(query);
            return positions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPositionTotalDifferenceCount() => Connection.PositionTotalDifferences.Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetDebutName(byte[] key)
        {
            var debut = Connection.Debuts.FirstOrDefault(d => d.Sequence == key);
            return debut?.Name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Debut> GetAllDebuts() => Connection.Debuts.ToList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddDebuts(IEnumerable<Debut> debuts)
        {
            Connection.Debuts.AddRange(debuts);
            Connection.SaveChanges();
        }
    }
}
