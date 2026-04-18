namespace Analysis.Kio.Library.ViewModels;
#region Models

public class LibraryMoveStatModel : BindableBase
{
    public short Key { get; set; }
    
    private int _rank;
    public int Rank
    {
        get => _rank;
        set => SetProperty(ref _rank, value);
    }

    public string Move { get; set; } = string.Empty;
    
    public int Total { get; set; }
    public int WhiteWins { get; set; }
    public int Draws { get; set; }
    public int BlackWins { get; set; }
    
    public double WhitePercent { get; set; }
    public double DrawPercent { get; set; }
    public double BlackPercent { get; set; }
    
    public short PercentDiff { get; set; }

    public string TotalDisplay => Total.ToString("N0");
    public string WhitePercentDisplay => $"{WhitePercent:F1}%";
    public string DrawPercentDisplay => $"{DrawPercent:F1}%";
    public string BlackPercentDisplay => $"{BlackPercent:F1}%";
    public string PercentDiffDisplay => PercentDiff >= 0 ? $"+{PercentDiff}" : PercentDiff.ToString();
    
    public bool HasData => Total > 0;
}

#endregion
