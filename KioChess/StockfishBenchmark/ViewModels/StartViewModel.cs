using System.Windows.Input;

namespace StockfishBenchmark.ViewModels;

public class StartViewModel : BindableBase
{
    public ICommand OpenPerformanceCommand { get; }
    public ICommand OpenTtStatisticsCommand { get; }

    public StartViewModel(Action<BindableBase> navigate)
    {
        OpenPerformanceCommand  = new DelegateCommand(() => navigate(new SetupViewModel(navigate)));
        OpenTtStatisticsCommand = new DelegateCommand(() => navigate(new TtSetupViewModel(navigate)));
    }
}
