namespace Engine.Tools;

public class PerformanceItem
{
    public int Count { get; set; }
    public TimeSpan Time { get; set; }
    public double Average { get; set; }

    internal void Calculate() => Average = Math.Round(1.0 * Time.Ticks / Count, 3);
}
