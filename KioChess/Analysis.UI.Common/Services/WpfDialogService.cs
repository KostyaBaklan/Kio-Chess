using Analysis.UI.Common.Views;
using Microsoft.Win32;
using System.Windows;

namespace Analysis.UI.Common.Services;

/// <summary>
/// WPF implementation of dialog service for MVVM.
/// </summary>
public class WpfDialogService : IDialogService
{
    public void ShowTextInput(string title, string message, string defaultValue, string okButtonText, Action<string> callback, bool allowMultiline = false)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var viewModel = new TextInputDialogViewModel
            {
                Title = title,
                Message = message,
                InputText = defaultValue,
                OkButtonText = okButtonText,
                AllowMultiline = allowMultiline
            };

            var dialog = new TextInputDialog
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            viewModel.CloseRequested += (result) =>
            {
                dialog.DialogResult = result;
                dialog.Close();
            };

            var dialogResult = dialog.ShowDialog();
            callback(dialogResult == true ? viewModel.InputText : null);
        });
    }

    public void ShowOpenFile(string title, string filter, string defaultExt, Action<string> callback)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExt
            };

            var result = dialog.ShowDialog();
            callback(result == true ? dialog.FileName : null);
        });
    }

    public void ShowSaveFile(string title, string filter, string defaultExt, string defaultFileName, Action<string> callback)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExt,
                FileName = defaultFileName
            };

            var result = dialog.ShowDialog();
            callback(result == true ? dialog.FileName : null);
        });
    }
}
