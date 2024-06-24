

namespace StockfishApp.Models
{
    public class StockFishLog
    {
        public string Color { get; set; }
        public int Elo { get; set; }
        public short Depth { get; set; }
        public short StDepth { get; set; }
        public string Strategy { get; set; }
        public string History { get; set; }
        public string Opening { get; set; }
        public string Error { get; set; }
    }
}
