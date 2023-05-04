using CommonServiceLocator;
using Engine.Interfaces.Config;
using Engine.Interfaces;
using Engine.Models.Config;
using Engine.Services.Bits;
using Engine.Services;
using Newtonsoft.Json;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UI.Common.Interfaces;
using UI.Common.Services;
using System.IO;

namespace UI.Common.App
{
    public abstract class AppBase : PrismApplication
    {
        protected override Window CreateShell()
        {
            RegisterRegions();

            return new Shell();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var s = File.ReadAllText(@"Config\Configuration.json");
            Configuration configuration = JsonConvert.DeserializeObject<Configuration>(s);

            var x = File.ReadAllText(@"Config\StaticTables.json");
            StaticTableCollection collection = JsonConvert.DeserializeObject<StaticTableCollection>(x);

            var z = File.ReadAllText(@"Config\Table.json");
            Dictionary<int, TableConfiguration> table = JsonConvert.DeserializeObject<Dictionary<int, TableConfiguration>>(z);

            Evaluation evaluation = configuration.Evaluation;
            IConfigurationProvider configurationProvider = new ConfigurationProvider(configuration.AlgorithmConfiguration, new EvaluationProvider(evaluation.Static, evaluation.Opening, evaluation.Middle, evaluation.End),
                configuration.GeneralConfiguration, configuration.PieceOrderConfiguration, configuration.EndGameConfiguration);
            containerRegistry.RegisterInstance(configurationProvider);

            IStaticValueProvider staticValueProvider = new StaticValueProvider(collection);
            containerRegistry.RegisterInstance(staticValueProvider);

            ITableConfigurationProvider tableConfigurationProvider = new TableConfigurationProvider(table, configurationProvider);
            containerRegistry.RegisterInstance(tableConfigurationProvider);

            containerRegistry.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
            containerRegistry.RegisterSingleton(typeof(IMoveSorterProvider), typeof(MoveSorterProvider));
            containerRegistry.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
            containerRegistry.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
            containerRegistry.RegisterSingleton(typeof(IEvaluationService), typeof(EvaluationService));
            containerRegistry.RegisterSingleton(typeof(IKillerMoveCollectionFactory), typeof(KillerMoveCollectionFactory));
            containerRegistry.RegisterSingleton(typeof(IAttackEvaluationService), typeof(AttackEvaluationService));
            containerRegistry.RegisterSingleton(typeof(IOpeningService), typeof(OpeningService));
            containerRegistry.RegisterSingleton(typeof(IProbCutModelProvider), typeof(ProbCutModelProvider));
            containerRegistry.RegisterSingleton(typeof(ITranspositionTableService), typeof(TranspositionTableService));
            containerRegistry.RegisterSingleton(typeof(IDataPoolService), typeof(DataPoolService));
            containerRegistry.RegisterSingleton(typeof(IStrategyFactory), typeof(StrategyFactory));

            if (ArmBase.Arm64.IsSupported)
            {
                containerRegistry.RegisterSingleton(typeof(IBitService), typeof(AmdBitService));
            }
            else if (Popcnt.X64.IsSupported && Bmi1.X64.IsSupported)
            {
                containerRegistry.RegisterSingleton(typeof(IBitService), typeof(IntelBitService));
            }
            else
            {
                containerRegistry.RegisterSingleton(typeof(IBitService), typeof(BitService));
            }


            containerRegistry.RegisterSingleton(typeof(IStrategyProvider), typeof(StrategyProvider));

            RegisterInternal(containerRegistry);
        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.Assembly.FullName;
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);
                return Type.GetType(viewModelName);
            });

            ViewModelLocationProvider.SetDefaultViewModelFactory(vmType =>
            {
                var resolve = ServiceLocator.Current.GetInstance(vmType);
                return resolve;
            });
        }

        protected abstract void RegisterRegions();

        protected abstract void RegisterInternal(IContainerRegistry containerRegistry);
    }
}
