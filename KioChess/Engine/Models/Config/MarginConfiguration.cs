namespace Engine.Models.Config;

public class MarginConfiguration
{
    public int[] AttackMargin { get; set; }
    public int[] FutilityMargin { get; set; }
    public int[] ReverseFutilityMargin { get; set; }
    public int[] RazoringMargin { get; set; }
    public int TradeMargin { get; set; }
    public int DeltaPruningMargin { get; set; }
}
