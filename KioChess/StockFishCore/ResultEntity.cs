namespace StockFishCore
{
    public class ResultEntity
    {
        public int Id { get; set; }
        public int Skill { get; set; }
        public short Depth { get; set; }
        public short StockFishDepth { get; set; }
        public string Strategy { get; set; }
        public string Color { get; set; }
        public StockFishGameResultType Result { get; set; }

        public DateTime Time { get; set; }
    }
}