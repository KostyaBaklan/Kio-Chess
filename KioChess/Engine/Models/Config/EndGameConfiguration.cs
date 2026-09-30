using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class EndGameConfiguration : IEndGameConfiguration
{
    public sbyte[] EndGameDepthOffset { get; set; }

    public sbyte[] EndGameDepth { get; set; }

    public int EndGameSearchExtension { get; set; }

    public sbyte[] EndGameDepthExtension { get; set; }
}