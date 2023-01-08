using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Kgb.ChessApp.Views
{
    public class StartViewModel : BindableBase
    {
        public StartViewModel()
        {
            Colors = new[] { "White", "Black" };
            Levels = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
            Color = Colors.FirstOrDefault();
            Level = Levels.First();

            PlayCommand = new DelegateCommand(PlayExecute);
        }

        public IEnumerable<string> Colors { get; }
        private string _color;

        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public IEnumerable<int> Levels { get; }
        private int _level;

        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public ICommand PlayCommand { get; }

        private void PlayExecute()
        {
            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            var navigationParameters = new NavigationParameters { { "Color", _color }, { "Level", _level + 1 } };
            regionManager.RequestNavigate("Main", typeof(GameView).Name, navigationParameters);
        }
    }
}