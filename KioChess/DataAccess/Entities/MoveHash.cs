using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

public class MoveHash
{
    public short Id { get; set; }
    public ulong Low { get; set; }
    public ulong High { get; set; }

    [NotMapped]
    public UInt128 Hash
    {
        get => new UInt128(High, Low);
        set
        {
            Low = (ulong)value;
            High = (ulong)(value >> 64);
        }
    }
}
