using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.DataStructures;

public class ResultDto
{
    public int Value { get; set; }
    public GameResult GameResult { get; set; }
    public short Move { get; set; }
}

public class Result : IResult
{
    public Result()
    {
        Value = int.MinValue;
        GameResult = GameResult.Continue;
    }

    #region Implementation of IResult

    public int Value { get; set; }
    public GameResult GameResult { get; set; }
    public MoveBase Move { get; set; }

    public ResultDto Todto() => new ResultDto { Value = Value, GameResult = GameResult.Continue, Move = (short)(Move == null ? -1 : Move.Key) };

    #endregion
}