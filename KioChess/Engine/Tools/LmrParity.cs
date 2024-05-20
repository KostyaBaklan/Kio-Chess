namespace Engine.Tools
{
    public class LmrParity
    {
        public int Depth { get; set; }
        public SortedDictionary<int, List<LmrParityItem>> Items { get; set; } 
            = new SortedDictionary<int, List<LmrParityItem>>();

        internal void Add(short ply, int depth, int index, int count)
        {
            LmrParityItem lmrParityItem = new LmrParityItem { Depth = depth, Index = index, Count = count };
            if (Items.TryGetValue(ply, out var list))
            {
                list.Add(lmrParityItem);
            }
            else
            {
                Items[ply] = new List<LmrParityItem> { lmrParityItem };
            }
        }
    }
}
