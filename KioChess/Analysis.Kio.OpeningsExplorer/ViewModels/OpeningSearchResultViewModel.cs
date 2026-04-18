namespace Analysis.Kio.OpeningsExplorer.ViewModels;

public class OpeningSearchResultViewModel
{
    public string Name { get; set; } = string.Empty;
    public string ECO { get; set; } = string.Empty;
    public string Moves { get; set; } = string.Empty;
    public string MovesUCI { get; set; } = string.Empty;
    public int Popularity { get; set; }
    public int OpeningId { get; set; }
    
    public string Display => string.IsNullOrEmpty(ECO) 
        ? Name 
        : $"[{ECO}] {Name}";
}
