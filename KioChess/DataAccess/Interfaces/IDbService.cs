﻿using System.Data.Common;

namespace DataAccess.Interfaces;

public interface IDbService
{
    void Connect();
    void Disconnect();
    void Execute(string sql, int timeout = 30);
    IEnumerable<T> Execute<T>(string sql, Func<DbDataReader, T> factoy = null);
}
