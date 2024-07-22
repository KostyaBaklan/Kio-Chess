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
        public double Duration { get; set; }
        public double MoveTime { get; set; }
        public int RunTimeId { get; set; }
    }
}