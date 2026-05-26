using Analysis.Core.Interfaces;
using Analysis.Core.Services;
using Analysis.DataAccess.Interfaces;
using Analysis.DataAccess.Services;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Hash;
using System.Windows;
using UI.Common;

namespace Analysis.UI.Common;

/// <summary>
/// Base application class for all Analysis.Kio.* applications.
/// Provides common DI registration, theme management, and database connections.
/// </summary>
public abstract class AnalysisApp : UiApp
{
    /// <summary>
    /// Creates the main window. Override in derived app to create the specific shell window.
    /// </summary>
    protected abstract override Window CreateShell();

    /// <summary>
    /// Registers application-specific types. Override in derived app to add view models.
    /// </summary>
    protected abstract void RegisterAppTypes(IContainerRegistry containerRegistry);

    protected override void ConfigureViewModelLocator()
    {
        base.ConfigureViewModelLocator();

        // Map Views to ViewModels by convention
        ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
        {
            var viewName = viewType.FullName ?? string.Empty;
            var vmName = viewName
                .Replace(".Views.", ".ViewModels.")
                .Replace(".Views,", ".ViewModels,");
            if (!vmName.EndsWith("ViewModel", StringComparison.Ordinal))
                vmName += "Model";
            return Type.GetType($"{vmName}, {viewType.Assembly.FullName}");
        });
    }

    protected sealed override void RegisterLocalTypes(IContainerRegistry containerRegistry)
    {
        // Core engine components - Position is singleton per app
        containerRegistry.RegisterSingleton<Position>();

        // Common ViewModels
        containerRegistry.Register<ViewModels.SettingsViewModel>();

        // Common infrastructure services
        containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();
        containerRegistry.RegisterSingleton<ISoundService, Analysis.UI.Common.Services.SoundService>();
        containerRegistry.RegisterSingleton<Services.ThemeService>();
        containerRegistry.RegisterSingleton<Services.IDialogService, Services.WpfDialogService>();
        containerRegistry.RegisterSingleton<IStockfishService, StockfishService>();
        containerRegistry.RegisterSingleton<IAnalysisService, AnalysisService>();
        containerRegistry.RegisterSingleton<IOpeningExplorerService, OpeningExplorerService>();
        containerRegistry.RegisterSingleton<IPgnParserService, PgnParserService>();

        // Let derived app register its specific types
        RegisterAppTypes(containerRegistry);
    }

    protected virtual void RegisterDialogs(IContainerRegistry containerRegistry)
    {
        // Reserved for future Prism dialog registration if needed
    }

    protected override void OnInitialized()
    {
        // Load persisted settings and apply the saved theme
        var settings = ContainerLocator.Current.Resolve<ISettingsService>();
        settings.Load();

        base.OnInitialized();

        var themeService = ContainerLocator.Current.Resolve<Services.ThemeService>();
        themeService.ApplyByKey(settings.Current.Theme);

        var sound = ContainerLocator.Current.Resolve<ISoundService>();
        sound.SetEnabled(settings.Current.SoundEnabled);
        sound.SetVolume(settings.Current.SoundVolume);
    }

    protected override void DbConnect()
    {
        var appDb = ContainerLocator.Current.Resolve<IAppDbService>();
        appDb.Connect(); 
        var hash = appDb.GetAllMoveHashValues();
        MoveHashSequenceHasher.Initialize(hash);

        var gameDb = ContainerLocator.Current.Resolve<IGameDbService>();
        gameDb.Connect();

        var openingDb = ContainerLocator.Current.Resolve<IOpeningDbService>();
        openingDb.Connect();

        var localDb = ContainerLocator.Current.Resolve<ILocalDbService>();
        localDb.Connect();

        gameDb.LoadAsync();
    }

    protected override void DbDisconnect()
    {
        ContainerLocator.Current.Resolve<IAppDbService>().Disconnect();
        ContainerLocator.Current.Resolve<IGameDbService>().Disconnect();
        ContainerLocator.Current.Resolve<IOpeningDbService>().Disconnect();
        ContainerLocator.Current.Resolve<ILocalDbService>().Disconnect();
    }
}
