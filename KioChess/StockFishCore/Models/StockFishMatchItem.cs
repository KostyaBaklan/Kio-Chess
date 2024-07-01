namespace StockFishCore
{
    public class StockFishMatchItem
    {
        public StockFishResultItem StockFishResultItem { get; set; }
        public double Kio { get; set; }
        public double SF { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Looses { get; set; }
        public double Duration { get; set; }
    }
}