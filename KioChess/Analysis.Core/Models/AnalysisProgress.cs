namespace Analysis.Core.Models;

public class AnalysisProgress
{
    public int CurrentMove { get; set; }
    public int TotalMoves { get; set; }
    public string Status { get; set; } = string.Empty;
    public double PercentComplete => TotalMoves > 0 ? (double)CurrentMove / TotalMoves * 100 : 0;
}
