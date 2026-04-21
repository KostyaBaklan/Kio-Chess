namespace Analysis.UI.Common.Services;

/// <summary>
/// Service for showing dialogs in a ViewModel-first MVVM approach.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a text input dialog.
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Prompt message</param>
    /// <param name="defaultValue">Default input value</param>
    /// <param name="okButtonText">OK button text</param>
    /// <param name="callback">Callback with result (null if cancelled)</param>
    /// <param name="allowMultiline">Allow multi-line input</param>
    void ShowTextInput(string title, string message, string defaultValue, string okButtonText, Action<string> callback, bool allowMultiline = false);

    /// <summary>
    /// Shows an open file dialog.
    /// </summary>
    void ShowOpenFile(string title, string filter, string defaultExt, Action<string> callback);

    /// <summary>
    /// Shows a save file dialog.
    /// </summary>
    void ShowSaveFile(string title, string filter, string defaultExt, string defaultFileName, Action<string> callback);
}

