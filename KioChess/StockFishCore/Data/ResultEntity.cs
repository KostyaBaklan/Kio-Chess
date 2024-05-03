using Engine.Models.Enums;

namespace StockFishCore.Data
{
    public class ResultEntity
    {
        public int Id { get; set; }
        public int Skill { get; set; }
        public short Depth { get; set; }
        public short StockFishDepth { get; set; }
        public StrategyType Strategy { get; set; }
        public string Color { get; set; }
        public StockFishGameResultType Result { get; set; }

        public double KioValue { get; set; }
        public double SfValue { get; set; }
        public string Sequence { get; set; }

        public DateTime Time { get; set; }
    }
}