using Analysis.Core.Models;
using Analysis.Kio.Play.Views;
using System.Windows;

namespace Analysis.Kio.Play.ViewModels;

/// <summary>
/// ViewModel for the New Game dialog.
/// </summary>
public class NewGameDialogViewModel : BindableBase
{
    public NewGameDialogViewModel()
    {
        EloProfiles = EloProfile.Presets;
        _selectedProfile = EloProfile.Presets[5]; // Experienced default

        ConfirmCommand = new DelegateCommand(OnConfirm);
        CancelCommand = new DelegateCommand(OnCancel);
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
    public DelegateCommand CancelCommand { get; }

    public bool Confirmed { get; private set; }

    private void OnConfirm()
    {
        Confirmed = true;
        CloseDialog(true);
    }

    private void OnCancel()
    {
        Confirmed = false;
        CloseDialog(false);
    }

    private static void CloseDialog(bool result)
    {
        foreach (Window w in Application.Current.Windows)
        {
            if (w is NewGameDialog dlg)
            {
                dlg.DialogResult = result;
                return;
            }
        }
    }
}
