using System.Windows;

namespace Analysis.UI.Common.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();

        // Manually create and set ViewModel with proper DI
        var vm = ContainerLocator.Current.Resolve<ViewModels.SettingsViewModel>();
        DataContext = vm;

        // Handle dialog result from ViewModel
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.DialogResult))
            {
                DialogResult = vm.DialogResult;
                if (vm.DialogResult)
                {
                    Close();
                }
            }
        };
    }
}