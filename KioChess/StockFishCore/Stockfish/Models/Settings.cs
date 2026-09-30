namespace StockFishCore.Stockfish.Models
{
    public class Settings
    {
        public int Hash { get; set; } = 32;
        public int Threads { get; set; } = 1;
        public bool Ponder { get; set; } = false;
        public int MultiPV { get; set; } = 1;
        public int Elo { get; set; }
        public bool UCIChess960 { get; set; } = false;

        public Settings(int elo)
        {
            Elo = elo;
        }

        public Dictionary<string, string> GetPropertiesAsDictionary() => new()
        {
            ["Threads"] = Threads.ToString(),                       // 1
            ["Hash"] = Hash.ToString(),                             // must follow Threads
            ["Ponder"] = Ponder.ToString().ToLowerInvariant(),
            ["MultiPV"] = MultiPV.ToString(),
            ["UCI_Chess960"] = UCIChess960.ToString().ToLowerInvariant(),
            ["UCI_LimitStrength"] = false.ToString().ToLowerInvariant(),
            ["UCI_Elo"] = Elo.ToString()
        };
    }
}
