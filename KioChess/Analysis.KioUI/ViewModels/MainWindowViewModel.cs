using Analysis.Core.Models;
using Analysis.KioUI.Services;

namespace Analysis.KioUI.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly ThemeService _themeService;

    public MainWindowViewModel(ThemeService themeService)
    {
        _themeService = themeService;
        _activeTheme = _themeService.ActiveTheme;
        _statusText = "Ready";
        _engineStatusText = "Engine: offline";
    }

    public IReadOnlyList<ThemeInfo> Themes => ThemeInfo.All;

    private ThemeInfo _activeTheme;
    public ThemeInfo ActiveTheme
    {
        get => _activeTheme;
        set
        {
            if (SetProperty(ref _activeTheme, value) && value is not null)
                _themeService.Apply(value);
        }
    }

    private string _statusText;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private string _engineStatusText;
    public string EngineStatusText
    {
        get => _engineStatusText;
        set => SetProperty(ref _engineStatusText, value);
    }
}
