namespace Engine.Interfaces.Config
{
    public class SortingConfiguration
    {
        public bool UseSortHard { get; set; }
        public int[] SortDepth { get; set; }
        public int[] SortHardDepth { get; set; }
        public bool UseSortDifference { get; set; }
        public int[] SortDifferenceDepth { get; set; }
        public SortType SortType { get; set; }
        public int SortMinimum { get; set; }
    }
}