namespace DataAccess.Models
{
    public class HistoryTotalItem
    {
        public string History;
        public short Key;
        public int Total;

        public override string ToString()
        {
            return $"[{Key},{Total}]";
        }
    }
    public class SequenceTotalItem
    {
        public string Seuquence;
        public BookMove Move;
    }
}
