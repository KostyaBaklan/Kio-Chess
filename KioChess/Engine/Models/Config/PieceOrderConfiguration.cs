using Engine.Models.Enums;

namespace Engine.Models.Config
{
    public class PieceOrderConfiguration : IPieceOrderConfiguration
    {
        public Dictionary<Phase, Piece[]> Whites { get; set; }
        public Dictionary<Phase, Piece[]> Blacks { get; set; }
        public Dictionary<Phase, Piece[]> WhitesAttacks { get; set; }
        public Dictionary<Phase, Piece[]> BlacksAttacks { get; set; }
    }
}