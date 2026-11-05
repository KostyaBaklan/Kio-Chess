using TexelTool.Models;

namespace TexelTool.Configuration
{
    public class BoundsConfiguration
    {
        public int KingSafetyMaxValue { get; set; }
        public int PassedPawnMaxValue { get; set; }
        public int SupportPassedPawnMaxValue { get; set; } = 40;
        public int ScalarMinValue { get; set; }
        public int ScalarMaxValue { get; set; }
        public ValueRange PstRange { get; set; } = new() { Min = -100, Max = 100 };
        public PstRangesConfiguration PstRanges { get; set; } = new();
        public PieceRangesConfiguration PieceRanges { get; set; } = new();
        public Dictionary<string, int> ScalarGroups { get; set; } = new();
        public Dictionary<string, ValueRange> ScalarRanges { get; set; } = new();
        public Dictionary<string, string> ScalarGroupMap { get; set; } = new();
        public Dictionary<string, int> KingSafetyGroups { get; set; } = new();
        public Dictionary<string, string> KingSafetyGroupMap { get; set; } = new();
        public List<string> FrozenKingSafety { get; set; } = new();
        public int MaxAttackWeightStep { get; set; } = 40;
        public List<ScalarOrderRule> ScalarOrderRules { get; set; } = new();
        public PieceRulesConfiguration PieceRules { get; set; } = new();
    }

}
