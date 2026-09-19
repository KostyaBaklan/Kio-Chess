using DataAccess.Entities;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services.BulkInsert;

public class PopularPositionEntityBulkInsertService : BulkInsertServiceBase<PopularPositionEntity>
{
    protected override void CreateParameters(SqliteCommand command)
    {
        command.Parameters.AddWithValue("$HL", 0L);
        command.Parameters.AddWithValue("$HH", 0L);
        command.Parameters.AddWithValue("$M", 0);
        command.Parameters.AddWithValue("$T", 0);
        command.Parameters.AddWithValue("$L", 0);
    }

    protected override string GetSql()
    {
    return @"INSERT INTO PopularPositions(HashLow, HashHigh, NextMove, Total, Length) 
                      VALUES($HL, $HH, $M, $T, $L)
                      ON CONFLICT(HashLow, HashHigh, NextMove) DO UPDATE 
                      SET Total = Total + excluded.Total,
                          Length = MIN(Length, excluded.Length)";
    }

    protected override void SetValues(PopularPositionEntity record, SqliteCommand command)
    {
        command.Parameters[0].Value = (long)record.HashLow;
        command.Parameters[1].Value = (long)record.HashHigh;
        command.Parameters[2].Value = record.NextMove;
        command.Parameters[3].Value = record.Total;
        command.Parameters[4].Value = record.Length;
    }
}
