using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.DataStructures
{
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

        #endregion
    }
}