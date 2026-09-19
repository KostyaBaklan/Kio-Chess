using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using System.Text.Json;

namespace Analysis.Core.Services;

public class SettingsService : ISettingsService
{
    private const string FileName = "Config\\appsettings.json";

    private static readonly JsonSerializerOptions _writeOptions = new() { WriteIndented = true };

    public AppSettings Current { get; private set; } = new();

    public void Load()
    {
        if (!File.Exists(FileName))
        {
            Current = new AppSettings();
            return;
        }

        try
        {
            var json = File.ReadAllText(FileName);
            var app = JsonSerializer.Deserialize<AppSettings>(json);
            Current = app ?? new AppSettings();
        }
        catch
        {
            Current = new AppSettings();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Current, _writeOptions);
        File.WriteAllText(FileName, json);
    }
}
