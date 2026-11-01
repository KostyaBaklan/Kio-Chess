using Engine.Models.Boards;
using System.Runtime.InteropServices;

namespace TexelTool;

internal sealed class TexelDataSet
{
    private List<ulong> _pieceList = new();
    private List<byte> _phaseList = new();
    private List<byte> _castleList = new();
    private List<float> _resultList = new();

    public ulong[] Pieces { get; private set; }
    public byte[] Phases { get; private set; }
    public byte[] Castles { get; private set; }
    public float[] Results { get; private set; }
    public int[][] PhaseIndices { get; private set; }

    public int Count => Results?.Length ?? _resultList.Count;

    public void Add(Board board, byte phase, byte castle, float result)
    {
        for (byte p = 0; p < 12; p++)
        {
            _pieceList.Add(board.GetPieceBits(p));
        }

        _phaseList.Add(phase);
        _castleList.Add(castle);
        _resultList.Add(result);
    }

    private const int Magic = 0x54455845;

    public void Save(string path, double k)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20);
        using var writer = new BinaryWriter(stream);
        writer.Write(Magic);
        writer.Write(Count);
        writer.Write(k);
        writer.Flush();
        stream.Write(MemoryMarshal.AsBytes(Pieces.AsSpan()));
        stream.Write(Phases);
        stream.Write(Castles);
        stream.Write(MemoryMarshal.AsBytes(Results.AsSpan()));
    }

    public static TexelDataSet Load(string path, out double k)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
        using var reader = new BinaryReader(stream);
        if (reader.ReadInt32() != Magic) throw new InvalidDataException("Not a Texel dataset file: " + path);

        int count = reader.ReadInt32();
        k = reader.ReadDouble();

        var data = new TexelDataSet
        {
            _pieceList = null,
            _phaseList = null,
            _castleList = null,
            _resultList = null,
            Pieces = new ulong[count * 12],
            Phases = new byte[count],
            Castles = new byte[count],
            Results = new float[count]
        };

        stream.ReadExactly(MemoryMarshal.AsBytes(data.Pieces.AsSpan()));
        stream.ReadExactly(data.Phases);
        stream.ReadExactly(data.Castles);
        stream.ReadExactly(MemoryMarshal.AsBytes(data.Results.AsSpan()));
        data.BuildPhaseIndices();
        return data;
    }

    public void Truncate(int count)
    {
        _pieceList.RemoveRange(count * 12, _pieceList.Count - count * 12);
        _phaseList.RemoveRange(count, _phaseList.Count - count);
        _castleList.RemoveRange(count, _castleList.Count - count);
        _resultList.RemoveRange(count, _resultList.Count - count);
    }

    public void Freeze()
    {
        Pieces = _pieceList.ToArray();
        Phases = _phaseList.ToArray();
        Castles = _castleList.ToArray();
        Results = _resultList.ToArray();
        _pieceList = null;
        _phaseList = null;
        _castleList = null;
        _resultList = null;
        BuildPhaseIndices();
    }

    public ReadOnlySpan<ulong> PiecesOf(int index) => new(Pieces, index * 12, 12);

    public void Keep(bool[] keep)
    {
        var pieces = new List<ulong>();
        var phases = new List<byte>();
        var castles = new List<byte>();
        var results = new List<float>();

        for (int i = 0; i < keep.Length; i++)
        {
            if (!keep[i]) continue;

            for (int k = 0; k < 12; k++) pieces.Add(Pieces[i * 12 + k]);
            phases.Add(Phases[i]);
            castles.Add(Castles[i]);
            results.Add(Results[i]);
        }

        Pieces = pieces.ToArray();
        Phases = phases.ToArray();
        Castles = castles.ToArray();
        Results = results.ToArray();
        BuildPhaseIndices();
    }

    private void BuildPhaseIndices()
    {
        PhaseIndices = new int[3][];
        for (int phase = 0; phase < 3; phase++)
        {
            var list = new List<int>();
            for (int i = 0; i < Phases.Length; i++)
            {
                if (Phases[i] == phase) list.Add(i);
            }
            PhaseIndices[phase] = list.ToArray();
        }
    }
}
