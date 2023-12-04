namespace Engine.Models.Config;

public class PieceOrderConfiguration : IPieceOrderConfiguration
{
    public Dictionary<byte, byte[]> Whites { get; set; }
    public Dictionary<byte, byte[]> Blacks { get; set; }
    public Dictionary<byte, byte[]> WhitesAttacks { get; set; }
    public Dictionary<byte, byte[]> BlacksAttacks { get; set; }
}