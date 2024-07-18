namespace StockFishCore
{
    public class StockFishItem
    {
        public double Kio { get; set; }
        public double SF { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Looses { get; set; }
        public double Duration { get; set; }
        public double MoveTime { get; set; }
    }
    public class StockFishMatchItem
    {
        public StockFishResultItem StockFishResultItem { get; set; }
        public StockFishItem Result { get; set; }
    }
    public class StockFishCompareItem
    {
        public StockFishResultItem StockFishResultItem { get; set; }
        public StockFishItem Left { get; set; }
        public StockFishItem Right { get; set; }
    }
}