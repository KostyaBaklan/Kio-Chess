using Engine.Strategies.Base;

namespace Engine.Strategies.Models;

public class AspirationModel
{
    public int Window { get; set; }
    public sbyte Depth { get; set; }
    public StrategyBase Strategy { get; set; }
}