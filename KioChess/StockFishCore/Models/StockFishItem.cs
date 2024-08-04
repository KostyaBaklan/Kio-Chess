
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

        public double WinPercentage => Math.Round(100.0 * Wins / Total, 1);

        public double NonLoosePercentage => Math.Round(100.0 * (Wins+Draws) / Total, 1);

        public double Total => Wins + Draws + Looses;
    }
}