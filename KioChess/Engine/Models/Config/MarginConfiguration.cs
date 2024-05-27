
namespace Engine.Models.Config;

public class MarginConfiguration
{
    public int AttackMargin { get; set; }
    public int[] PromotionMargins { get; set; }
    public int[][] AlphaMargins { get; set; }
    public int[][] BetaMargins { get; set; }
    public int[] DeltaMargins { get; set; }
}
