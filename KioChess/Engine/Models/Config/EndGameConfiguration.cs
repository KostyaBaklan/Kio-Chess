using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class EndGameConfiguration : IEndGameConfiguration
{
    public short MaxEndGameDepth { get; set; }

    public short EndGameDepthOffset { get; set; }
}