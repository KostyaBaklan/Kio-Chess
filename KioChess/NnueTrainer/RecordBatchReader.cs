using System.Buffers.Binary;
using System.Numerics;

namespace NnueTrainer;

/// <summary>Sequential batch reader over 32-byte records with per-epoch shuffled batch order.</summary>
internal sealed class RecordBatchReader : IDisposable
{
    public const int RecordSize = 32;
    public const int Inputs = 768;

    private readonly FileStream _stream;
    private readonly byte[] _buffer;
    private readonly int _batchSize;

    public RecordBatchReader(string path, int batchSize)
    {
        _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
        _batchSize = batchSize;
        _buffer = new byte[batchSize * RecordSize];
        Count = _stream.Length / RecordSize;
        Batches = (int)((Count + batchSize - 1) / batchSize);
    }

    public long Count { get; }
    public int Batches { get; }

    public Batch Read(int batchIndex, double lambda, double scale)
    {
        _stream.Position = (long)batchIndex * _batchSize * RecordSize;
        int bytes = _stream.ReadAtLeast(_buffer, _buffer.Length, false);
        int count = bytes / RecordSize;

        var batch = new Batch(count);
        int features = 0;

        for (int i = 0; i < count; i++)
        {
            var record = new ReadOnlySpan<byte>(_buffer, i * RecordSize, RecordSize);
            ulong occupancy = BinaryPrimitives.ReadUInt64LittleEndian(record);
            bool black = record[27] == 1;

            batch.StmOffsets[i] = features;
            batch.NstmOffsets[i] = features;

            int n = 0;
            for (ulong bits = occupancy; bits != 0; bits &= bits - 1, n++)
            {
                int square = BitOperations.TrailingZeroCount(bits);
                int piece = (record[8 + (n >> 1)] >> ((n & 1) * 4)) & 0xF;

                long white = piece * 64 + square;
                long blackView = ((piece + 6) % 12) * 64 + (square ^ 56);

                batch.StmIndices.Add(black ? blackView : white);
                batch.NstmIndices.Add(black ? white : blackView);
                features++;
            }

            short score = BinaryPrimitives.ReadInt16LittleEndian(record[24..]);
            double wdl = record[26] * 0.5;
            double searchTarget = 1.0 / (1.0 + Math.Exp(-score / scale));
            batch.Targets[i] = (float)(lambda * searchTarget + (1.0 - lambda) * wdl);
        }

        return batch;
    }

    public void Dispose() => _stream.Dispose();
}

internal sealed class Batch
{
    public Batch(int count)
    {
        Count = count;
        StmOffsets = new long[count];
        NstmOffsets = new long[count];
        Targets = new float[count];
        StmIndices = new List<long>(count * 32);
        NstmIndices = new List<long>(count * 32);
    }

    public int Count { get; }
    public long[] StmOffsets { get; }
    public long[] NstmOffsets { get; }
    public float[] Targets { get; }
    public List<long> StmIndices { get; }
    public List<long> NstmIndices { get; }
}
