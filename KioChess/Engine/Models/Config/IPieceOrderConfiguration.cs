using Engine.Models.Enums;

namespace Engine.Models.Config
{
    public interface IPieceOrderConfiguration
    {
        Dictionary<byte, Piece[]> Blacks { get; }
        Dictionary<byte, Piece[]> BlacksAttacks { get;  }
        Dictionary<byte, Piece[]> Whites { get;  }
        Dictionary<byte, Piece[]> WhitesAttacks { get; }
    }
}