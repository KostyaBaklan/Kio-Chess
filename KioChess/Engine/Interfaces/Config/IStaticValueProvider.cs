using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Interfaces.Config
{
    public interface IStaticValueProvider
    {
        int GetValue(byte piece, byte phase, byte square);
    }
}