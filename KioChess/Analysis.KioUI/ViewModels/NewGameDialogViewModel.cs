using Analysis.Core.Models;
using System.Windows;

namespace Analysis.KioUI.ViewModels;

/// <summary>
/// ViewModel for the New Game dialog.
/// The dialog is shown modally from <see cref="PlayViewModel"/>.
/// </summary>
public class NewGameDialogViewModel : BindableBase
{
    public NewGameDialogViewModel()
    {
        EloProfiles = EloProfile.Presets;
        _selectedProfile = EloProfile.Presets[2]; // Intermediate default

        ConfirmCommand = new DelegateCommand(OnConfirm);
        CancelCommand  = new DelegateCommand(OnCancel);
    }

    public IReadOnlyList<EloProfile> EloProfiles { get; }

    private EloProfile _selectedProfile;
    public EloProfile SelectedProfile
    {
        get => _selectedProfile;
        set => SetProperty(ref _selectedProfile, value);
    }

    private bool _playAsWhite = true;
    public bool PlayAsWhite
    {
        get => _playAsWhite;
        set => SetProperty(ref _playAsWhite, value);
    }

    public DelegateCommand ConfirmCommand { get; }
    public DelegateCommand CancelCommand  { get; }

    public bool Confirmed { get; private set; }

    private void OnConfirm()
    {
        Confirmed = true;
        CloseDialog();
    }

    private void OnCancel()
    {
        Confirmed = false;
        foreach (Window w in Application.Current.Windows)
        {
            if (w is Views.NewGameDialog dlg)
            {
                dlg.DialogResult = false;
                return;
            }
        }
    }

    private static void CloseDialog()
    {
        foreach (Window w in Application.Current.Windows)
        {
            if (w is Views.NewGameDialog dlg)
            {
                dlg.DialogResult = true;
                return;
            }
        }
    }
}
