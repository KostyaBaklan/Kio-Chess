using Engine.Models.Enums;

namespace Engine.Models.Config
{
    public class PieceOrderConfiguration : IPieceOrderConfiguration
    {
        public Dictionary<byte, Piece[]> Whites { get; set; }
        public Dictionary<byte, Piece[]> Blacks { get; set; }
        public Dictionary<byte, Piece[]> WhitesAttacks { get; set; }
        public Dictionary<byte, Piece[]> BlacksAttacks { get; set; }
    }
}