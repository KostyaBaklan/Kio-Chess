using Engine.Models.Boards;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace TexelTool
{
    internal sealed unsafe class TexelDataSet : IDisposable
    {
        private List<ulong> _pieceList = new();
        private List<byte> _phaseList = new();
        private List<byte> _castleList = new();
        private List<float> _resultList = new();
        private List<int> _countList = new();
        private Dictionary<ulong, int> _seen = new();

        public long Duplicates { get; private set; }

        private ulong[] _pieces;
        private byte[] _phases;
        private byte[] _castles;
        private float[] _results;

        private MemoryMappedFile _file;
        private MemoryMappedViewAccessor _view;
        private byte* _viewBase;
        private int _mappedCount;
        private ulong* _piecePtr;
        private float* _resultPtr;
        private byte* _phasePtr;
        private byte* _castlePtr;

        public ReadOnlySpan<ulong> Pieces => _pieces != null ? new ReadOnlySpan<ulong>(_pieces) : new ReadOnlySpan<ulong>(_piecePtr, _mappedCount * 12);
        public ReadOnlySpan<byte> Phases => _phases != null ? new ReadOnlySpan<byte>(_phases) : new ReadOnlySpan<byte>(_phasePtr, _mappedCount);
        public ReadOnlySpan<byte> Castles => _castles != null ? new ReadOnlySpan<byte>(_castles) : new ReadOnlySpan<byte>(_castlePtr, _mappedCount);
        public ReadOnlySpan<float> Results => _results != null ? new ReadOnlySpan<float>(_results) : new ReadOnlySpan<float>(_resultPtr, _mappedCount);
        public int[][] PhaseIndices { get; private set; }

        public static int ValidationEvery;            // 0 = disabled; every N-th block of 256 positions is held out
        public int[][] ValidationIndices { get; private set; }
        public int TrainCount { get; private set; }
        public int ValidationCount { get; private set; }

        public int Count => _results?.Length ?? (_resultList?.Count ?? _mappedCount);

        public void Add(Board board, byte phase, byte castle, float result)
        {
            Span<ulong> bits = stackalloc ulong[12];
            ulong hash = 0xcbf29ce484222325UL ^ castle;
            for (byte p = 0; p < 12; p++)
            {
                bits[p] = board.GetPieceBits(p);
                hash = (hash ^ bits[p]) * 0x100000001b3UL;
                hash ^= hash >> 29;
                hash *= 0xbf58476d1ce4e5b9UL;
            }

            if (_seen.TryGetValue(hash, out var existing))
            {
                _resultList[existing] += result;
                _countList[existing]++;
                Duplicates++;
                return;
            }

            _seen[hash] = _resultList.Count;

            for (byte p = 0; p < 12; p++)
            {
                _pieceList.Add(bits[p]);
            }

            _phaseList.Add(phase);
            _castleList.Add(castle);
            _resultList.Add(result);
            _countList.Add(1);
        }

        private const int Magic = 0x54455846;
        private const int HeaderSize = 16;

        // Layout: header (16), pieces (8-byte aligned), results, phases, castles.
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
            stream.Write(MemoryMarshal.AsBytes(Pieces));
            stream.Write(MemoryMarshal.AsBytes(Results));
            stream.Write(Phases);
            stream.Write(Castles);
        }

        /// <summary>Maps the file into memory; processes mapping the same file share physical pages.</summary>
        public static TexelDataSet Load(string path, out double k)
        {
            int count;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                if (reader.ReadInt32() != Magic) throw new InvalidDataException("Not a Texel dataset file (or old format, re-extract): " + path);
                count = reader.ReadInt32();
                k = reader.ReadDouble();
            }

            var data = new TexelDataSet
            {
                _pieceList = null,
                _phaseList = null,
                _castleList = null,
                _resultList = null,
                _countList = null,
                _seen = null,
                _mappedCount = count
            };

            data._file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            data._view = data._file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            byte* pointer = null;
            data._view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
            data._viewBase = pointer + data._view.PointerOffset;

            long offset = HeaderSize;
            data._piecePtr = (ulong*)(data._viewBase + offset);
            offset += (long)count * 12 * sizeof(ulong);
            data._resultPtr = (float*)(data._viewBase + offset);
            offset += (long)count * sizeof(float);
            data._phasePtr = data._viewBase + offset;
            offset += count;
            data._castlePtr = data._viewBase + offset;

            data.BuildPhaseIndices();
            return data;
        }

        public void Dispose()
        {
            if (_view == null) return;
            _view.SafeMemoryMappedViewHandle.ReleasePointer();
            _view.Dispose();
            _file.Dispose();
            _view = null;
            _file = null;
        }

        public void Truncate(int count)
        {
            _pieceList.RemoveRange(count * 12, _pieceList.Count - count * 12);
            _phaseList.RemoveRange(count, _phaseList.Count - count);
            _castleList.RemoveRange(count, _castleList.Count - count);
            _resultList.RemoveRange(count, _resultList.Count - count);
            _countList.RemoveRange(count, _countList.Count - count);
        }

        public void Freeze()
        {
            _pieces = _pieceList.ToArray();
            _phases = _phaseList.ToArray();
            _castles = _castleList.ToArray();
            _results = _resultList.ToArray();
            for (int i = 0; i < _results.Length; i++) _results[i] /= _countList[i];
            _pieceList = null;
            _phaseList = null;
            _castleList = null;
            _resultList = null;
            _countList = null;
            _seen = null;
            BuildPhaseIndices();
        }

        public ReadOnlySpan<ulong> PiecesOf(int index) => Pieces.Slice(index * 12, 12);

        public void Keep(bool[] keep)
        {
            int kept = 0;
            for (int i = 0; i < keep.Length; i++) if (keep[i]) kept++;
            var pieces = new ulong[kept * 12];
            var phases = new byte[kept];
            var castles = new byte[kept];
            var results = new float[kept];
            var oldPieces = Pieces;
            var oldPhases = Phases;
            var oldCastles = Castles;
            var oldResults = Results;
            int n = 0;
            for (int i = 0; i < keep.Length; i++)
            {
                if (!keep[i]) continue;
                oldPieces.Slice(i * 12, 12).CopyTo(pieces.AsSpan(n * 12, 12));
                phases[n] = oldPhases[i];
                castles[n] = oldCastles[i];
                results[n] = oldResults[i];
                n++;
            }
            _pieces = pieces;
            _phases = phases;
            _castles = castles;
            _results = results;
            BuildPhaseIndices();
        }

        private static bool IsHeldOut(int i) => ValidationEvery > 0 && (i >> 8) % ValidationEvery == 0;

        private void BuildPhaseIndices()
        {
            var phases = Phases;
            var trainCount = new int[2];
            var validCount = new int[2];
            for (int i = 0; i < phases.Length; i++)
            {
                if (IsHeldOut(i)) validCount[phases[i]]++;
                else trainCount[phases[i]]++;
            }
            PhaseIndices = new int[2][];
            ValidationIndices = new int[2][];
            for (int p = 0; p < 2; p++)
            {
                PhaseIndices[p] = new int[trainCount[p]];
                ValidationIndices[p] = new int[validCount[p]];
                trainCount[p] = 0;
                validCount[p] = 0;
            }
            for (int i = 0; i < phases.Length; i++)
            {
                int p = phases[i];
                if (IsHeldOut(i)) ValidationIndices[p][validCount[p]++] = i;
                else PhaseIndices[p][trainCount[p]++] = i;
            }
            TrainCount = Math.Max(1, PhaseIndices.Sum(a => a.Length));
            ValidationCount = ValidationIndices.Sum(a => a.Length);
        }
    }
}
