using TexelTool.Models;

namespace TexelTool.Configuration
{
    public class PstRangesConfiguration
    {
        public ValueRange Pawn { get; set; }
        public ValueRange Knight { get; set; }
        public ValueRange Bishop { get; set; }
        public ValueRange Rook { get; set; }
        public ValueRange Queen { get; set; }
        public ValueRange King { get; set; }
    }
}
