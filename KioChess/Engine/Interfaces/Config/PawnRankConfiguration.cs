
namespace Engine.Interfaces.Config
{
    public class PawnRankConfiguration
    {
        public byte[] WhiteOpening { get; set; }
        public byte[] WhiteMiddle { get; set; }
        public byte[] WhiteEnd { get; set; }
        public byte[] BlackOpening { get; set; }
        public byte[] BlackMiddle { get; set; }
        public byte[] BlackEnd { get; set; }
    }

    public class PassedPawnConfiguration
    {
        public PawnRankConfiguration Passed { get; set; }
        public PawnRankConfiguration Candidates { get; set; }
    }
}
