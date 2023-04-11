namespace Engine.Models.Config
{
    public interface IPieceOrderConfiguration
    {
        Dictionary<byte, byte[]> Blacks { get; }
        Dictionary<byte, byte[]> BlacksAttacks { get;  }
        Dictionary<byte, byte[]> Whites { get;  }
        Dictionary<byte, byte[]> WhitesAttacks { get; }
    }
}