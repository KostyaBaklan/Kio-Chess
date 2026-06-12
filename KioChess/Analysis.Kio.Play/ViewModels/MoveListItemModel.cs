namespace Analysis.Kio.Play.ViewModels;

public class MoveListItemModel : BindableBase
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
}
