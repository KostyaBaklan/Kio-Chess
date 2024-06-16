namespace Engine.Interfaces.Config
{
    public class MobilityConfiguration
    {
        public PhaseMobilityConfiguration[] Phases { get; set; }
    }

    public class PieceMobilityConfiguration
    {
        public int Value { get; set; }
        public int ZeroPenalty { get; set; }

    }

    public class PhaseMobilityConfiguration
    {
        public PieceMobilityConfiguration Knight { get; set; }
        public PieceMobilityConfiguration Bishop { get; set; }
        public PieceMobilityConfiguration Rook { get; set; }
    }
}
