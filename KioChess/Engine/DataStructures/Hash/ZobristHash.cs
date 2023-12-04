using System.Runtime.CompilerServices;
using Engine.Models.Boards;
using Engine.Models.Helpers;

namespace Engine.DataStructures.Hash;

public class ZobristHash
{
    public ulong Key;
    private readonly ulong[][] _table;

    public ZobristHash()
    {
        HashSet<ulong> set = new HashSet<ulong>();

        _table = new ulong[64][];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _table[i * 8 + j] = new ulong[12];
                for (int k = 0; k < 12; k++)
                {
                    var x = RandomHelpers.NextLong();
                    while (!set.Add(x))
                    {
                        x = RandomHelpers.NextLong();
                    }

                    _table[i * 8 + j][k] = x;
                }
            }
        }
    }

    public void Initialize(BitBoard[] map)
    {
        Key = 0L;
        for (byte index = 0; index < map.Length; index++)
        {
            var set = map[index];
            foreach (var b in set.BitScan())
            {
                Key = Key ^ _table[b][index];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(byte coordinate, int figure)
    {
        Key = Key ^ _table[coordinate][figure];
    }

    public void Update(byte from, byte to, byte figure)
    {
        Key = Key ^ _table[from][figure] ^ _table[to][figure];
    }
}
