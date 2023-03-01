namespace TestViewer.Views
{
    public class TestResultViewModel
    {
        public TestResultViewModel(string category, string min, string max)
        {
            Category = category;
            Min = min;
            Max = max;
        }

        public string Category { get; }

        public string Min { get; }

        public string Max { get; }
    }
}