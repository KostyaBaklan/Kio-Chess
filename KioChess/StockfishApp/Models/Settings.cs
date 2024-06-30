namespace StockfishApp.Models
{
    public class Settings
    {
        public int Contempt { get; set; } = 0;
        public int Threads { get; set; } = 0;
        public bool Ponder { get; set; } = false;
        public int MultiPV { get; set; } = 1;
        public int Elo { get; set; }
        public int MoveOverhead { get; set; } = 30;
        public int SlowMover { get; set; } = 80;
        public bool UCIChess960 { get; set; } = false;

        public Settings(
            int elo
        )
        {
            Elo = elo;
        }

        public Dictionary<string, string> GetPropertiesAsDictionary() => new Dictionary<string, string>
        {
            ["Contempt"] = Contempt.ToString(),
            ["Threads"] = Threads.ToString(),
            ["Ponder"] = Ponder.ToString(),
            ["MultiPV"] = MultiPV.ToString(),
            ["Move Overhead"] = MoveOverhead.ToString(),
            ["Slow Mover"] = SlowMover.ToString(),
            ["UCI_Chess960"] = UCIChess960.ToString(),
            ["UCI_LimitStrength"] = true.ToString(),
            ["UCI_Elo"] = Elo.ToString()
        };
    }
}