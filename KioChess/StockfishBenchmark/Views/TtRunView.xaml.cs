using System.Windows.Controls;

namespace StockfishBenchmark.Views;

public partial class TtRunView : UserControl
{
    public TtRunView()
    {
        InitializeComponent();
    }

    private void MovesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MovesGrid.SelectedItem != null)
            MovesGrid.ScrollIntoView(MovesGrid.SelectedItem);
    }
}
