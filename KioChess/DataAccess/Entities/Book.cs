using ProtoBuf;

namespace DataAccess.Entities;

[ProtoContract]
public class Book:IEquatable<Book>
{
    [ProtoMember(1)]
    public byte[] History { get; set; }

    [ProtoMember(2)]
    public short NextMove { get; set; }

    [ProtoMember(3)]
    public int White { get; set; }

    [ProtoMember(4)]
    public int Draw { get; set; }

    [ProtoMember(5)]
    public int Black { get; set; }

    public bool Equals(Book other)
    {
        return History.SequenceEqual(other.History) && NextMove == other.NextMove;
    }
}
