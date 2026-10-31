using Engine.Models.Boards;
using System.Buffers.Binary;
using System.Numerics;

namespace NnueTool;

/// <summary>
/// 32-byte packed record (little endian):
/// 0..7 occupancy, 8..23 4-bit piece codes in occupancy bit order, 24..25 STM score,
/// 26 wdl (0 loss / 1 draw / 2 win, STM), 27 stm (0 white / 1 black), 28 ply, 29 depth, 30..31 reserved.
/// </summary>
internal static class TrainingRecord
{
    public const int Size = 32;
    public const int KeySize = 24;
    public const int StmOffset = 27;
    public const int ScoreOffset = 24;
    public const int WdlOffset = 26;

    public static bool Encode(Board board, bool whiteToMove, short score, byte wdl, byte ply, byte depth, Span<byte> record)
    {
        record.Clear();

        Span<byte> squares = stackalloc byte[64];
        ulong occupancy = 0;

        for (byte piece = 0; piece < 12; piece++)
        {
            ulong bits = board.GetPieceBits(piece);
            occupancy |= bits;

            for (; bits != 0; bits &= bits - 1)
            {
                squares[BitOperations.TrailingZeroCount(bits)] = piece;
            }
        }

        if (BitOperations.PopCount(occupancy) > 32)
            return false;

        BinaryPrimitives.WriteUInt64LittleEndian(record, occupancy);

        int n = 0;
        for (ulong bits = occupancy; bits != 0; bits &= bits - 1, n++)
        {
            byte piece = squares[BitOperations.TrailingZeroCount(bits)];
            record[8 + (n >> 1)] |= (byte)(piece << ((n & 1) * 4));
        }

        BinaryPrimitives.WriteInt16LittleEndian(record[ScoreOffset..], score);
        record[WdlOffset] = wdl;
        record[StmOffset] = whiteToMove ? (byte)0 : (byte)1;
        record[28] = ply;
        record[29] = depth;
        return true;
    }

    public static ulong PositionKey(ReadOnlySpan<byte> record)
    {
        ulong hash = 14695981039346656037UL;
        for (int i = 0; i < KeySize; i++)
        {
            hash = (hash ^ record[i]) * 1099511628211UL;
        }
        return (hash ^ record[StmOffset]) * 1099511628211UL;
    }
}
