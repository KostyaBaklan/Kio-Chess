using DataAccess.Entities;
using Microsoft.Data.Sqlite;

namespace DataAccess.Services.BulkInsert;

public class GameEntityBulkInsertService : BulkInsertServiceBase<GameEntity>
{
    protected override void CreateParameters(SqliteCommand command)
    {
        command.Parameters.AddWithValue("$L", 0L);
        command.Parameters.AddWithValue("$H", 0L);
        command.Parameters.AddWithValue("$M", 0);
        command.Parameters.AddWithValue("$W", 0);
        command.Parameters.AddWithValue("$D", 0);
        command.Parameters.AddWithValue("$B", 0);
        command.Parameters.AddWithValue("$LEN", 0);
    }

    protected override string GetSql()
    {
        return @"INSERT INTO GameEntities(Low, High, NextMove, White, Draw, Black, Length) 
                      VALUES($L, $H, $M, $W, $D, $B, $LEN)
                      ON CONFLICT(Low, High, NextMove) DO UPDATE 
                      SET White = White + excluded.White,
                          Draw = Draw + excluded.Draw,
                          Black = Black + excluded.Black,
                          Length = MIN(Length, excluded.Length)";
    }

    protected override void SetValues(GameEntity record, SqliteCommand command)
    {
        command.Parameters[0].Value = (long)record.Low;
        command.Parameters[1].Value = (long)record.High;
        command.Parameters[2].Value = record.NextMove;
        command.Parameters[3].Value = record.White;
        command.Parameters[4].Value = record.Draw;
        command.Parameters[5].Value = record.Black;
        command.Parameters[6].Value = record.Length;
    }
}
