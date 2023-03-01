using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Common;
using Newtonsoft.Json;
using Prism.Commands;

namespace TestViewer.Views
{
    public class TestTabItem : IEquatable<TestTabItem>
    {
        public TestTabItem(int depth, string game)
        {
            Depth = depth;
            Game = game;
            Name = $"{Depth}_{Game}";
        }

        public int Depth { get; }
        public string Game { get; }
        public string Name { get; }

        #region Overrides of Object

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestTabItem) obj);
        }

        #region Equality members

        public bool Equals(TestTabItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TestTabItem left, TestTabItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TestTabItem left, TestTabItem right)
        {
            return !Equals(left, right);
        }

        #endregion

        #endregion
    }

    public class TestViewModel
    {
        public TestViewModel()
        {
            var files = Directory.GetFiles(@"..\net6.0\Log", "*.log", SearchOption.TopDirectoryOnly);
            List<TestModel> models = new List<TestModel>(files.Length);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var testModel = JsonConvert.DeserializeObject<TestModel>(content);
                models.Add(testModel);
            }

            var map = models.GroupBy(m =>new TestTabItem(m.Depth, m.Game))
                .ToDictionary(k => k.Key, v => v.ToList());

            var t = new List<TestItemViewModel>();

            foreach (var pair in map)
            {
                var ti = new List<TestModel>();
                foreach (var testModel in pair.Value)
                {
                    ti.Add(testModel);
                }

                TestItemViewModel testItemViewModel = new TestItemViewModel(pair.Key,ti);
                t.Add(testItemViewModel);
            }

            Tests = new List<TestItemViewModel>(t);

            LoadedCommand = new DelegateCommand(OnLoaded);
        }

        public ICollection<TestItemViewModel> Tests { get; }

        public ICommand LoadedCommand { get; }

        private void OnLoaded()
        {
            var tests = Tests.ToArray();
            foreach (var testItemViewModel in tests)
            {
                testItemViewModel.CreateResults();
            }
        }
    }
}
