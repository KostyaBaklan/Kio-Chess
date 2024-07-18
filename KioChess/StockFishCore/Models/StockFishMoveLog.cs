namespace StockFishCore.Models
{
    public class StockFishMoveLog
    {
        public string BestMove { get; set; }
        public List<string> Moves { get; set; }
        public List<string> UCI { get; set; }
    }
}
