using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.UI.Common.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Analysis.UI.Common.ViewModels;

/// <summary>
/// ViewModel for application settings dialog.
/// Manages Stockfish path, theme selection, sound settings, and animation preferences.
/// </summary>
public class SettingsViewModel : BindableBase
{
    private readonly ISettingsService _settingsService;
    private readonly ThemeService _themeService;
    private readonly ISoundService _soundService;

    public SettingsViewModel(
        ISettingsService settingsService,
        ThemeService themeService,
        ISoundService soundService)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        _soundService = soundService;

        Themes = ThemeInfo.All;
        PieceSets = PieceSet.All;
        AnimationSpeeds = ["Fast", "Normal", "Slow", "Off"];

        // Reload settings to ensure we have the latest values
        _settingsService.Load();

        // Load current settings
        var s = _settingsService.Current;
        _stockfishPath = s.StockfishPath;
        _soundEnabled = s.SoundEnabled;
        _soundVolume = s.SoundVolume;
        _animationSpeed = s.AnimationSpeed;
        _selectedTheme = ThemeInfo.All.FirstOrDefault(t => t.Key == s.Theme) ?? ThemeInfo.All[0];
        _selectedPieceSet = PieceSet.All.FirstOrDefault(p => p.Key == s.PieceSet) ?? PieceSet.Default;

        BrowseStockfishCommand = new DelegateCommand(OnBrowseStockfish);
        TestStockfishCommand = new DelegateCommand(OnTestStockfish, CanTestStockfish);
        SaveCommand = new DelegateCommand(OnSave);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    public DelegateCommand BrowseStockfishCommand { get; }
    public DelegateCommand TestStockfishCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public IReadOnlyList<ThemeInfo> Themes { get; }
    public IReadOnlyList<PieceSet> PieceSets { get; }
    public IReadOnlyList<string> AnimationSpeeds { get; }

    private string _stockfishPath;
    public string StockfishPath
    {
        get => _stockfishPath;
        set
        {
            if (SetProperty(ref _stockfishPath, value))
            {
                TestStockfishCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private ThemeInfo _selectedTheme;
    public ThemeInfo SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value) && value != null)
            {
                _themeService.ApplyByKey(value.Key);
            }
        }
    }

    private PieceSet _selectedPieceSet;
    public PieceSet SelectedPieceSet
    {
        get => _selectedPieceSet;
        set
        {
            if (SetProperty(ref _selectedPieceSet, value) && value != null)
            {
                Application.Current.Resources["ActivePieceSet"] = value.Key;
            }
        }
    }

    private bool _soundEnabled;
    public bool SoundEnabled
    {
        get => _soundEnabled;
        set
        {
            if (SetProperty(ref _soundEnabled, value))
            {
                _soundService.SetEnabled(value);
            }
        }
    }

    private double _soundVolume;
    public double SoundVolume
    {
        get => _soundVolume;
        set
        {
            if (SetProperty(ref _soundVolume, value))
            {
                _soundService.SetVolume(value);
            }
        }
    }

    private string _animationSpeed;
    public string AnimationSpeed
    {
        get => _animationSpeed;
        set => SetProperty(ref _animationSpeed, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _dialogResult;
    public bool DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    private void OnBrowseStockfish()
    {
        var initialDir = !string.IsNullOrEmpty(StockfishPath) && File.Exists(StockfishPath)
            ? Path.GetDirectoryName(StockfishPath)
            : AppDomain.CurrentDomain.BaseDirectory;

        var dlg = new OpenFileDialog
        {
            Title = "Locate Stockfish Executable",
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            FileName = !string.IsNullOrEmpty(StockfishPath) ? Path.GetFileName(StockfishPath) : "stockfish.exe",
            InitialDirectory = initialDir
        };

        if (dlg.ShowDialog() == true)
        {
            StockfishPath = dlg.FileName;
            StatusMessage = "Path updated. Click Save to apply.";
        }
    }

    private bool CanTestStockfish() => !string.IsNullOrEmpty(StockfishPath);

    private void OnTestStockfish()
    {
        if (string.IsNullOrEmpty(StockfishPath))
        {
            StatusMessage = "Please specify a Stockfish path first.";
            return;
        }

        if (!File.Exists(StockfishPath))
        {
            StatusMessage = "? File not found!";
            return;
        }

        try
        {
            // Try to start Stockfish and check if it responds
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = StockfishPath,
                Arguments = "",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.StandardInput.WriteLine("uci");
                process.StandardInput.WriteLine("quit");

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);
                process.Kill();
                process.Dispose();

                if (output.Contains("uciok"))
                {
                    StatusMessage = "? Stockfish is working correctly!";
                }
                else
                {
                    StatusMessage = "?? File runs but doesn't respond to UCI commands.";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
        }
    }

    private void OnSave()
    {
        var s = _settingsService.Current;
        s.StockfishPath = StockfishPath;
        s.Theme = _selectedTheme?.Key ?? "ClassicWood";
        s.PieceSet = _selectedPieceSet?.Key ?? "Classic";
        s.SoundEnabled = SoundEnabled;
        s.SoundVolume = SoundVolume;
        s.AnimationSpeed = AnimationSpeed;

        _settingsService.Save();

        StatusMessage = "? Settings saved successfully!";
        DialogResult = true;
    }

    private void OnCancel()
    {
        // Restore original theme and sound settings
        var s = _settingsService.Current;
        _themeService.ApplyByKey(s.Theme);
        _soundService.SetEnabled(s.SoundEnabled);
        _soundService.SetVolume(s.SoundVolume);

        if (s.PieceSet != null)
        {
            Application.Current.Resources["ActivePieceSet"] = s.PieceSet;
        }

        DialogResult = false;
    }
}
