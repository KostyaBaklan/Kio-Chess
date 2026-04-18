using Analysis.KioUI.ViewModels;

namespace Analysis.KioUI
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<MainWindowViewModel>();
        }
    }
}


