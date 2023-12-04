namespace DataAccess.Models;

public struct BookMove:IComparable<BookMove>
{
    public short Id;
    public int Value;

    public int CompareTo(BookMove other)
    {
        return other.Value.CompareTo(Value);
    }

    public override string ToString()
    {
        return $"[{Id},{Value}]";
    }
}
