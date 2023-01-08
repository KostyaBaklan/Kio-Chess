using Engine.Strategies.Base;

namespace Engine.Strategies.Models
{
    public class AspirationModel
    {
        public int Window { get; set; }
        public int Depth { get; set; }
        public MemoryStrategyBase Strategy { get; set; }
    }
}