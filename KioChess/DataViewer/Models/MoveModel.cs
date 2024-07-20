using Prism.Mvvm;

namespace DataViewer.Models;

public class MoveModel : BindableBase
{
    private int _number;

    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    private string _move;

    public string Move
    {
        get => _move;
        set => SetProperty(ref _move, value);
    }
}