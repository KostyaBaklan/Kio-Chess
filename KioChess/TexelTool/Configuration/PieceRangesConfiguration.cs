using TexelTool.Models;

namespace TexelTool.Configuration
{
    public class PieceRangesConfiguration
    {
        public ValueRange Knight { get; set; } = new() { Min = 260, Max = 400 };
        public ValueRange Bishop { get; set; } = new() { Min = 260, Max = 400 };
        public ValueRange Rook { get; set; } = new() { Min = 420, Max = 620 };
        public ValueRange Queen { get; set; } = new() { Min = 850, Max = 1100 };
    }

}
