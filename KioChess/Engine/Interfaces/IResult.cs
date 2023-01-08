using Engine.DataStructures;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IResult
    {
        int Value { get; }
        GameResult GameResult { get; }
        MoveBase Move { get; }
    }
}