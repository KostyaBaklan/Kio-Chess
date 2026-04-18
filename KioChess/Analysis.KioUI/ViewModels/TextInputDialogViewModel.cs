namespace Analysis.KioUI.ViewModels;

public class TextInputDialogViewModel : BindableBase
{
    private string _title = "Input";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _instructions = "Enter text:";
    public string Instructions
    {
        get => _instructions;
        set => SetProperty(ref _instructions, value);
    }

    private string _inputText = string.Empty;
    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    public DelegateCommand<System.Windows.Window> ConfirmCommand { get; }
    public DelegateCommand<System.Windows.Window> CancelCommand { get; }

    public TextInputDialogViewModel()
    {
        ConfirmCommand = new DelegateCommand<System.Windows.Window>(OnConfirm);
        CancelCommand = new DelegateCommand<System.Windows.Window>(OnCancel);
    }

    private void OnConfirm(System.Windows.Window window)
    {
        if (window != null)
        {
            window.DialogResult = true;
            window.Close();
        }
    }

    private void OnCancel(System.Windows.Window window)
    {
        if (window != null)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}

