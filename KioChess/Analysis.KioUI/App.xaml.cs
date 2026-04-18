using Analysis.Core.Interfaces;
using Analysis.Core.Services;
using Analysis.DataAccess.Interfaces;
using Analysis.DataAccess.Services;
using Analysis.KioUI.Services;
using Analysis.KioUI.ViewModels;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using System.Windows;
using UI.Common;

namespace Analysis.KioUI
{
    public partial class App : UiApp
    {
        protected override Window CreateShell() => new MainWindow();

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            // Map Analysis.KioUI.Views.FooView  →  Analysis.KioUI.ViewModels.FooViewModel
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName ?? string.Empty;
                var vmName   = viewName
                    .Replace(".Views.", ".ViewModels.")
                    .Replace(".Views,",  ".ViewModels,");
                if (!vmName.EndsWith("ViewModel", StringComparison.Ordinal))
                    vmName += "Model";
                return Type.GetType($"{vmName}, {viewType.Assembly.FullName}");
            });
        }

        protected override void RegisterLocalTypes(IContainerRegistry containerRegistry)
        {
            // Core engine components (must be singletons to share state)
            containerRegistry.RegisterSingleton<Engine.Models.Boards.Position>();
            
            // Infrastructure services
            containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();
            containerRegistry.RegisterSingleton<ISoundService, WpfSoundService>();
            containerRegistry.RegisterSingleton<Services.IDialogService, Services.DialogService>();
            containerRegistry.RegisterSingleton<ThemeService>();
            containerRegistry.RegisterSingleton<IStockfishService, StockfishService>();
            containerRegistry.RegisterSingleton<IAnalysisService, AnalysisService>();
            
            // Opening Explorer
            containerRegistry.RegisterSingleton<IOpeningExplorerService, OpeningExplorerService>();

            // Shell + tab ViewModels
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            containerRegistry.RegisterSingleton<PlayViewModel>();
            containerRegistry.RegisterSingleton<AnalyseViewModel>();
            containerRegistry.RegisterSingleton<OpeningExplorerViewModel>();
            containerRegistry.RegisterSingleton<LibraryViewModel>();
            containerRegistry.RegisterSingleton<SettingsViewModel>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Load persisted settings and apply the saved theme
            var settings = ContainerLocator.Current.Resolve<ISettingsService>();
            settings.Load();

            var themeService = ContainerLocator.Current.Resolve<ThemeService>();
            themeService.ApplyByKey(settings.Current.Theme);

            var sound = ContainerLocator.Current.Resolve<ISoundService>();
            sound.SetEnabled(settings.Current.SoundEnabled);
            sound.SetVolume(settings.Current.SoundVolume);
        }

        protected override void DbConnect()
        {
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
            ContainerLocator.Current.Resolve<IGameDbService>().Disconnect();
            ContainerLocator.Current.Resolve<IOpeningDbService>().Disconnect();
            ContainerLocator.Current.Resolve<ILocalDbService>().Disconnect();
        }
    }
}

