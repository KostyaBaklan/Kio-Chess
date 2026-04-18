namespace Analysis.KioUI.ViewModels;

/// <summary>One row in the move-list panel (move number + White notation + Black notation).</summary>
public class MoveListItemViewModel : BindableBase
{
    private int _number;
    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    private string _white = string.Empty;
    public string White
    {
        get => _white;
        set => SetProperty(ref _white, value);
    }

    private string _black = string.Empty;
    public string Black
    {
        get => _black;
        set => SetProperty(ref _black, value);
    }

    private string _whiteClassification = string.Empty;
    public string WhiteClassification
    {
        get => _whiteClassification;
        set => SetProperty(ref _whiteClassification, value);
    }

    private string _blackClassification = string.Empty;
    public string BlackClassification
    {
        get => _blackClassification;
        set => SetProperty(ref _blackClassification, value);
    }
}
