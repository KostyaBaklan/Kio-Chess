using Analysis.KioUI.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Analysis.KioUI.Views.Controls;

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

    // ── Cell click routing ───────────────────────────────────────

    private void OnCellMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not BoardViewModel vm) return;
        if (sender is not ContentPresenter cp) return;

        // The ContentPresenter's content is the CellViewModel
        if (cp.Content is CellViewModel cell)
            vm.HandleCellClick(cell);
    }
}
