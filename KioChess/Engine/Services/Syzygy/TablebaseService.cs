using DataAccess.Syzygy;
using Engine.Models.Boards;
using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Services.Syzygy;

/// <summary>
/// Engine-facing tablebase access. Converts the board to a SyzygyPosition and caches WDL results
/// in a lock-free, always-replace table of packed 64-bit entries (60 bits of hash + 4 bits of value),
/// so a torn read is impossible and repeated probes during search never reach the native layer.
/// </summary>
public sealed class TablebaseService : ITablebaseService
{
    private const int CacheBits = 19;
    private const int CacheMask = (1 << CacheBits) - 1;
    private const ulong KeyMask = ~0xFUL;
    private const ulong BlackKey = 0x9E3779B97F4A7C15UL;
    private const ulong Failed = 6;
    private const ulong EpKey = 0xC2B2AE3D27D4EB4FUL;

    private sealed class RootEntry
    {
        public ulong Key;
        public uint Rule50;
        public bool Found;
        public SyzygyRootResult Result;
    }

    private readonly ISyzygyService _syzygy;
    private readonly ulong[] _cache;
    private RootEntry _root;

    public TablebaseService(ISyzygyService syzygy)
    {
        _syzygy = syzygy;
        if (_syzygy.IsInitialized)
        {
            _cache = new ulong[1 << CacheBits];
        }
    }

    public bool IsEnabled => _syzygy.IsInitialized;

    public int MaxPieces => _syzygy.LargestTable;

    public bool TryProbeWdl(Board board, bool whiteToMove, int rule50, int enPassant, out TbResult wdl)
    {
        wdl = TbResult.Draw;

        if (rule50 != 0 || !_syzygy.IsInitialized || board.Occupied.Count() > _syzygy.LargestTable)
            return false;

        ulong hash = whiteToMove ? board.Hash : board.Hash ^ BlackKey;
        if (enPassant != 0) hash ^= (ulong)enPassant * EpKey;
        ulong key = hash & KeyMask;
        ref ulong slot = ref _cache[(int)(key >> 4) & CacheMask];
        ulong data = slot;

        if ((data & KeyMask) == key)
        {
            ulong value = data & 0xF;
            if (value == Failed) return false;
            if (value != 0)
            {
                wdl = (TbResult)(value - 1);
                return true;
            }
        }

        var position = CreatePosition(board, whiteToMove, 0, (uint)enPassant);
        if (!_syzygy.TryProbeWdl(position, out wdl))
        {
            slot = key | Failed;
            return false;
        }

        slot = key | ((ulong)wdl + 1);
        return true;
    }

    public bool TryProbeRoot(Board board, bool whiteToMove, int rule50, int enPassant, out SyzygyRootResult root)
    {
        root = SyzygyRootResult.Invalid;

        if (!_syzygy.IsInitialized || board.Occupied.Count() > _syzygy.LargestTable)
            return false;

        ulong key = whiteToMove ? board.Hash : board.Hash ^ BlackKey;
        if (enPassant != 0) key ^= (ulong)enPassant * EpKey;
        var cached = _root;
        if (cached != null && cached.Key == key && cached.Rule50 == (uint)rule50)
        {
            root = cached.Result;
            return cached.Found;
        }

        var result = _syzygy.ProbeRoot(CreatePosition(board, whiteToMove, (uint)rule50, (uint)enPassant));
        _root = new RootEntry { Key = key, Rule50 = (uint)rule50, Found = result.IsValid, Result = result };

        root = result;
        return result.IsValid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SyzygyPosition CreatePosition(Board board, bool whiteToMove, uint rule50, uint enPassant)
    {
        ulong knights = (ulong)board.GetPieceBits(Pieces.WhiteKnight) | (ulong)board.GetPieceBits(Pieces.BlackKnight);
        ulong bishops = (ulong)board.GetPieceBits(Pieces.WhiteBishop) | (ulong)board.GetPieceBits(Pieces.BlackBishop);
        ulong rooks = (ulong)board.GetPieceBits(Pieces.WhiteRook) | (ulong)board.GetPieceBits(Pieces.BlackRook);
        ulong queens = (ulong)board.GetPieceBits(Pieces.WhiteQueen) | (ulong)board.GetPieceBits(Pieces.BlackQueen);
        ulong kings = (ulong)board.GetPieceBits(Pieces.WhiteKing) | (ulong)board.GetPieceBits(Pieces.BlackKing);
        ulong pawns = (ulong)board.GetPieceBits(Pieces.WhitePawn) | (ulong)board.GetPieceBits(Pieces.BlackPawn);

        return new SyzygyPosition((ulong)board.Whites, (ulong)board.Blacks, kings, queens, rooks, bishops, knights, pawns,
            rule50, 0, enPassant, whiteToMove);
    }
}
