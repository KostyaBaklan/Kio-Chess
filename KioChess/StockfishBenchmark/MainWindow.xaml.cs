using StockfishBenchmark.ViewModels;
using System.Windows;

namespace StockfishBenchmark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<MainWindowViewModel>();
        }
    }
}