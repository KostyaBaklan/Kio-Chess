namespace StockfishBenchmark.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private BindableBase _currentView;

    public BindableBase CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public MainWindowViewModel()
    {
        Navigate(new SetupViewModel(Navigate));
    }

    public void Navigate(BindableBase vm) => CurrentView = vm;
}
