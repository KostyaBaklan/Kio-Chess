namespace Analysis.KioUI.Services;

/// <summary>
/// Service for showing modal dialogs in an MVVM-friendly way.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a text input dialog and returns the result.
    /// </summary>
    bool? ShowTextInputDialog(string title, string instructions, out string inputText);
}

public class DialogService : IDialogService
{
    public bool? ShowTextInputDialog(string title, string instructions, out string inputText)
    {
        var viewModel = new ViewModels.TextInputDialogViewModel
        {
            Title = title,
            Instructions = instructions
        };

        var dialog = new Views.TextInputDialogWindow
        {
            DataContext = viewModel,
            Owner = System.Windows.Application.Current.MainWindow
        };

        var result = dialog.ShowDialog();
        inputText = viewModel.InputText;
        
        return result;
    }
}
