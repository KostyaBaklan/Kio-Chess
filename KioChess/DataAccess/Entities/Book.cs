namespace DataAccess.Entities;

public class Book
{
    public byte[] History { get; set; } = null!;

    public short NextMove { get; set; }

    public int White { get; set; }

    public int Draw { get; set; }

    public int Black { get; set; }
}
