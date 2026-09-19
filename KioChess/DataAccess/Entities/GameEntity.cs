using ProtoBuf;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[ProtoContract]
public class GameEntity
{
    [ProtoMember(1)]
    public ulong Low { get; set; }

    [ProtoMember(2)]
    public ulong High { get; set; }

    [ProtoMember(3)]
    public short NextMove { get; set; }

    [ProtoMember(4)]
    public int White { get; set; }

    [ProtoMember(5)]
    public int Draw { get; set; }

    [ProtoMember(6)]
    public int Black { get; set; }

    [ProtoMember(7)]
    public byte Length { get; set; }

    [NotMapped]
    public UInt128 Hash
    {
        get => new(High, Low);
        set
        {
            Low = (ulong)value;
            High = (ulong)(value >> 64);
        }
    }
}
