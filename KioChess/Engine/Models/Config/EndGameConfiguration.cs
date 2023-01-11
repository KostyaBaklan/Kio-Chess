using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class EndGameConfiguration : IEndGameConfiguration
    {
        public int MaxEndGameDepth { get; set; }

        public int EndGameDepthOffset { get; set; }
    }
}