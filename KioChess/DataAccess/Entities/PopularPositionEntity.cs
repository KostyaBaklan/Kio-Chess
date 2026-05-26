using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

/// <summary>
/// Stores position data with 128-bit hash for in-memory cache (popular moves)
/// No length property needed - we only care about the unique sequence hash
/// </summary>
public class PopularPositionEntity
{
    public ulong HashLow { get; set; }
    public ulong HashHigh { get; set; }
    public short NextMove { get; set; }
    public int Total { get; set; }
    public byte Length { get; set; }

    [NotMapped]
    public UInt128 Hash
    {
        get => new UInt128(HashHigh, HashLow);
        set
        {
            HashLow = (ulong)value;
            HashHigh = (ulong)(value >> 64);
        }
    }
}
