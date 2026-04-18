using System.Windows;

namespace Analysis.KioUI.Views;

public partial class TextInputDialogWindow : Window
{
    public TextInputDialogWindow()
    {
        InitializeComponent();
        Loaded += (s, e) => InputTextBox.Focus();
    }
}
