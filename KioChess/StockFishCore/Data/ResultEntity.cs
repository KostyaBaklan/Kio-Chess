using Engine.Models.Enums;

namespace StockFishCore.Data
{
    public class ResultEntity
    {
        public int Id { get; set; }
        public int Elo { get; set; }
        public short Depth { get; set; }
        public short StockFishDepth { get; set; }
        public string Strategy { get; set; }
        public string Color { get; set; }
        public string Result { get; set; }

        public double KioValue { get; set; }
        public double SfValue { get; set; }
        public string Opening { get; set; }
        public string Sequence { get; set; }

        public DateTime Time { get; set; }
        public TimeSpan Duration { get;  set; }
        public string Branch { get; set; }
        public string Description { get; set; }

        public DateTime RunTime { get; set; }
    }
}