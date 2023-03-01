using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Common;
using Prism.Mvvm;

namespace TestViewer.Views
{
    public class TestItemViewModel : BindableBase
    {
        public TestItemViewModel(TestTabItem tab, List<TestModel> ti)
        {
            Tab = tab;
            TestItems = new List<TestModel>(ti);
        }

        public TestTabItem Tab { get; }

        public ICollection<TestModel> TestItems { get; }

        private ObservableCollection<TestResultViewModel> _testResults;

        public ObservableCollection<TestResultViewModel> TestResults
        {
            get => _testResults;
            set => SetProperty(ref _testResults, value);
        }

        public void CreateResults()
        {
            Dictionary<string, List<KeyValuePair<IComparable, string>>> category =
                new Dictionary<string, List<KeyValuePair<IComparable, string>>>
                {
                    {
                        "Total",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Total, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Min",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Min, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Max",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Max, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Average",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Average, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Std",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Std, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Table",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Table, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Evaluation",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Evaluation, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Memory",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                            new KeyValuePair<IComparable, string>(i.Memory, $"{i.Strategy}"))
                            .OrderBy(k=>k.Key))
                    },
                    {
                        "Material",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                                new KeyValuePair<IComparable, string>(i.Material, $"{i.Strategy}"))
                            .OrderByDescending(k=>k.Key))
                    },
                    {
                        "Static Value",
                        new List<KeyValuePair<IComparable, string>>(TestItems.Select(i =>
                                new KeyValuePair<IComparable, string>(i.StaticValue, $"{i.Strategy}"))
                            .OrderByDescending(k=>k.Key))
                    }
                };

            var testResultViewModels = category
                .Select(pair => new TestResultViewModel(pair.Key, pair.Value.First().Value, pair.Value.Last().Value))
                .ToArray();

            TestResults = new ObservableCollection<TestResultViewModel>();

            foreach (var testResultViewModel in testResultViewModels)
            {
                TestResults.Add(testResultViewModel);
            }
        }
    }
}