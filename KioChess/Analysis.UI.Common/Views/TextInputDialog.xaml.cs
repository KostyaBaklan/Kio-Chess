using System.Windows;

namespace Analysis.UI.Common.Views;

public partial class TextInputDialog : Window
{
    public TextInputDialog()
    {
        InitializeComponent();
    }
}

public class TextInputDialogViewModel : BindableBase
{
    public TextInputDialogViewModel()
    {
        OkCommand = new DelegateCommand(OnOk);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    public DelegateCommand OkCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public event Action<bool> CloseRequested;

    private string _title = "Input";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _message = "Enter value:";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private string _inputText = string.Empty;
    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    private string _okButtonText = "OK";
    public string OkButtonText
    {
        get => _okButtonText;
        set => SetProperty(ref _okButtonText, value);
    }

    private bool _allowMultiline = false;
    public bool AllowMultiline
    {
        get => _allowMultiline;
        set => SetProperty(ref _allowMultiline, value);
    }

    private void OnOk()
    {
        CloseRequested?.Invoke(true);
    }

    private void OnCancel()
    {
        CloseRequested?.Invoke(false);
    }
}





