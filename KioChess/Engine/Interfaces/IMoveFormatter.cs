using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IMoveFormatter
    {
        string Format(MoveBase move);
    }
}
