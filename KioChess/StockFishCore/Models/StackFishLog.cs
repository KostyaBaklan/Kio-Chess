namespace StockFishCore.Models
{
    public class StockFishLog
    {
        public string Color { get; set; }
        public int Elo { get; set; }
        public short Ply { get; set; }
        public short Depth { get; set; }
        public short StDepth { get; set; }
        public string Strategy { get; set; }
        public short[] Opening { get; set; }
        public short[] History { get; set; }
        public string Error { get; set; }
    }
}
