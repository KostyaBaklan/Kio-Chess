
namespace Engine.Models.Config;

public class MarginConfiguration
{
    public int[] AttackMargin { get; set; }
    public int[][] AlphaOffset { get; set; }
    public int[][] BetaOffset { get; set; }
    public int TradeMargin { get; set; }
}
