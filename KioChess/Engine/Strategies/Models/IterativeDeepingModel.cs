using Engine.Strategies.Base;

namespace Engine.Strategies.Models
{
    public class IterativeDeepingModel
    {
        public int Depth { get; set; }
        public StrategyBase Strategy { get; set; }
    }
}