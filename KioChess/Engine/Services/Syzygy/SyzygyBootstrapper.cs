using DataAccess.Syzygy;
using Engine.Models.Config;

namespace Engine.Services.Syzygy;

public static class SyzygyBootstrapper
{
    /// <summary>
    /// Initializes the process-wide Syzygy service from configuration. Returns it either way;
    /// when disabled or when initialization fails, IsInitialized stays false and the engine ignores tablebases.
    /// </summary>
    public static ISyzygyService Initialize(SyzygyConfiguration configuration)
    {
        var service = SyzygyService.Instance;

        if (configuration == null || !configuration.IsEnabled || string.IsNullOrWhiteSpace(configuration.TablesPath))
            return service;

        var dll = string.IsNullOrWhiteSpace(configuration.DllPath)
            ? Path.Combine(AppContext.BaseDirectory,"Libs", "fathomDll.dll")
            : configuration.DllPath;

        service.Initialize(dll, ExpandTablePaths(configuration.TablesPath));
        return service;
    }

    /// <summary>
    /// Fathom does not scan subfolders, so every directory that directly holds table files is listed explicitly.
    /// </summary>
    private static string ExpandTablePaths(string tablesPath)
    {
        if (tablesPath.Contains(';') || !Directory.Exists(tablesPath))
            return tablesPath;

        var directories = Directory
            .EnumerateFiles(tablesPath, "*.rtb?", SearchOption.AllDirectories)
            .Select(Path.GetDirectoryName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return directories.Length == 0 ? tablesPath : string.Join(';', directories);
    }
}
