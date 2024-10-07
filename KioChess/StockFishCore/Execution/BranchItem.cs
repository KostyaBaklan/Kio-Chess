namespace StockFishCore.Execution
{
    public class BranchItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Config { get; set; }

        public override string ToString()
        {
            return $"{Name}-{Description}";
        }
    }
}
