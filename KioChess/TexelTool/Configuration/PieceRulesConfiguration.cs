namespace TexelTool.Configuration
{
    public class PieceRulesConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int MaxMinorDifference { get; set; } = 50;
        public int MinBishopOverKnight { get; set; } = 0;
    }

}
