namespace DataAccess.Models;

public struct BookMove
{
    public short Id;
    public int Value;

    public override string ToString()
    {
        return $"[{Id},{Value}]";
    }
}
