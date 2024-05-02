namespace StockfishApp.Models
{
    public class Settings
    {
        public int Contempt { get; set; } = 0;
        public int Threads { get; set; } = 0;
        public bool Ponder { get; set; } = false;
        public int MultiPV { get; set; } = 1;
        public int SkillLevel { get; set; }
        public int MoveOverhead { get; set; } = 30;
        public int SlowMover { get; set; } = 80;
        public bool UCIChess960 { get; set; } = false;

        public Settings(
            int skillLevel
        )
        {
            SkillLevel = skillLevel;
        }

        public Dictionary<string, string> GetPropertiesAsDictionary()
        {
            return new Dictionary<string, string>
            {
                ["Contempt"] = Contempt.ToString(),
                ["Threads"] = Threads.ToString(),
                ["Ponder"] = Ponder.ToString(),
                ["MultiPV"] = MultiPV.ToString(),
                ["Skill Level"] = SkillLevel.ToString(),
                ["Move Overhead"] = MoveOverhead.ToString(),
                ["Slow Mover"] = SlowMover.ToString(),
                ["UCI_Chess960"] = UCIChess960.ToString(),
            };
        }
    }
}