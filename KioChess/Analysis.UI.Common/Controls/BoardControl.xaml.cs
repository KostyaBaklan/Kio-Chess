using Analysis.UI.Common.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Analysis.UI.Common.Controls;

/// <summary>
/// Reusable 8x8 chess board control.
/// DataContext must be a <see cref="BoardViewModel"/>.
/// </summary>
public partial class BoardControl : UserControl
{
    public BoardControl()
    {
        InitializeComponent();
    }

    private void OnCellMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not BoardViewModel vm) return;
        if (sender is not ContentPresenter cp) return;

        if (cp.Content is CellViewModel cell)
            vm.HandleCellClick(cell);
    }
}
