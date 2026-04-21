namespace Analysis.Kio.OpeningsExplorer.ViewModels;

public class OpeningMoveViewModel
{
    public string MoveUCI { get; set; } = string.Empty;
    public string MoveSAN { get; set; } = string.Empty;
    public string OpeningName { get; set; } = string.Empty;
    public string ECO { get; set; } = string.Empty;
    public int Popularity { get; set; }
    public bool IsMainLine { get; set; }
    
    public DelegateCommand<OpeningMoveViewModel> PlayMoveCommand { get; set; }
    
    public string PopularityDisplay => $"{Popularity}%";
    public string MoveDisplay => $"{MoveSAN}";
    public string FullDisplay => string.IsNullOrEmpty(ECO) 
        ? OpeningName 
        : $"{OpeningName} [{ECO}]";
}
