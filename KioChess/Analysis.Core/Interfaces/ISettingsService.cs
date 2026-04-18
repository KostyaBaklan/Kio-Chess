using Analysis.Core.Models;

namespace Analysis.Core.Interfaces;

public interface ISettingsService
{
    AppSettings Current { get; }
    void Save();
    void Load();
}
