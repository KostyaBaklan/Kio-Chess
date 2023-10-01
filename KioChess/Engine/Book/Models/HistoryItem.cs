
namespace Engine.Book.Models
{
    public  class HistoryItem
    {
        public string History;
        public short Key; 
        public int White;
        public int Draw;
        public int Black;
    }
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
