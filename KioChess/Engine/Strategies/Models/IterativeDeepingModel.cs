using Engine.Strategies.Base;

namespace Engine.Strategies.Models;

public class IterativeDeepingModel
{
    public sbyte Depth { get; set; }
    public MemoryStrategyBase Strategy { get; set; }
}