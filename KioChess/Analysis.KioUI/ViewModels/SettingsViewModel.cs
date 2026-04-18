using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.KioUI.Services;
using Microsoft.Win32;
using System.Windows;

namespace Analysis.KioUI.ViewModels;

public class SettingsViewModel : BindableBase
{
    private readonly ISettingsService _settingsService;
    private readonly ThemeService _themeService;

    public SettingsViewModel(ISettingsService settingsService, ThemeService themeService)
    {
        _settingsService = settingsService;
        _themeService = themeService;

        Themes         = ThemeInfo.All;
        PieceSets      = PieceSet.All;
        AnimationSpeeds = ["Fast", "Normal", "Off"];

        var s = _settingsService.Current;
        _stockfishPath  = s.StockfishPath;
        _soundEnabled   = s.SoundEnabled;
        _soundVolume    = s.SoundVolume;
        _animationSpeed = s.AnimationSpeed;
        _selectedTheme  = ThemeInfo.All.FirstOrDefault(t => t.Key == s.Theme)    ?? ThemeInfo.All[0];
        _selectedPieceSet = PieceSet.All.FirstOrDefault(p => p.Key == s.PieceSet) ?? PieceSet.Default;

        BrowseStockfishCommand = new DelegateCommand(OnBrowseStockfish);
        SelectThemeCommand     = new DelegateCommand<ThemeInfo>(OnSelectTheme);
        SaveCommand            = new DelegateCommand(OnSave);
    }

    // -- Commands ----------------------------------------------------------
    public DelegateCommand BrowseStockfishCommand  { get; }
    public DelegateCommand<ThemeInfo> SelectThemeCommand { get; }
    public DelegateCommand SaveCommand             { get; }

    // -- Collections -------------------------------------------------------
    public IReadOnlyList<ThemeInfo> Themes         { get; }
    public IReadOnlyList<PieceSet>  PieceSets      { get; }
    public IReadOnlyList<string>    AnimationSpeeds { get; }

    // -- Properties --------------------------------------------------------
    private string _stockfishPath;
    public string StockfishPath
    {
        get => _stockfishPath;
        set => SetProperty(ref _stockfishPath, value);
    }

    private ThemeInfo _selectedTheme;
    public ThemeInfo SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value) && value is not null)
                _themeService.Apply(value);
        }
    }

    private PieceSet _selectedPieceSet;
    public PieceSet SelectedPieceSet
    {
        get => _selectedPieceSet;
        set
        {
            if (SetProperty(ref _selectedPieceSet, value) && value is not null)
                Application.Current.Resources["ActivePieceSet"] = value.Key;
        }
    }

    private bool _soundEnabled;
    public bool SoundEnabled
    {
        get => _soundEnabled;
        set => SetProperty(ref _soundEnabled, value);
    }

    private double _soundVolume;
    public double SoundVolume
    {
        get => _soundVolume;
        set => SetProperty(ref _soundVolume, value);
    }

    private string _animationSpeed;
    public string AnimationSpeed
    {
        get => _animationSpeed;
        set => SetProperty(ref _animationSpeed, value);
    }

    private Visibility _savedNoticeVisibility = Visibility.Collapsed;
    public Visibility SavedNoticeVisibility
    {
        get => _savedNoticeVisibility;
        private set => SetProperty(ref _savedNoticeVisibility, value);
    }

    // -- Handlers ----------------------------------------------------------
    private void OnBrowseStockfish()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Locate Stockfish executable",
            Filter = "Executable|*.exe|All files|*.*",
            FileName = System.IO.Path.GetFileName(StockfishPath),
            InitialDirectory = System.IO.Path.GetDirectoryName(StockfishPath)
        };
        if (dlg.ShowDialog() == true)
            StockfishPath = dlg.FileName;
    }

    private void OnSelectTheme(ThemeInfo theme) => SelectedTheme = theme;

    private void OnSave()
    {
        var s = _settingsService.Current;
        s.StockfishPath  = StockfishPath;
        s.Theme          = _selectedTheme?.Key    ?? "ClassicWood";
        s.PieceSet       = _selectedPieceSet?.Key ?? "Classic";
        s.SoundEnabled   = SoundEnabled;
        s.SoundVolume    = SoundVolume;
        s.AnimationSpeed = AnimationSpeed;
        _settingsService.Save();

        SavedNoticeVisibility = Visibility.Visible;

        var t = new System.Windows.Threading.DispatcherTimer
            { Interval = TimeSpan.FromSeconds(3) };
        t.Tick += (_, _) => { SavedNoticeVisibility = Visibility.Collapsed; t.Stop(); };
        t.Start();
    }
}
