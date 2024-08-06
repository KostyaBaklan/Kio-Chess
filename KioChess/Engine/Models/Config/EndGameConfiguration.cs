using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class EndGameConfiguration : IEndGameConfiguration
{
    public short MaxEndGameDepth { get; set; }

    public sbyte[] EndGameDepthOffset { get; set; }
}