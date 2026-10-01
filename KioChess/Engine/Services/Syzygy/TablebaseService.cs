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
            Task.Run(WarmUp);
        }
    }

    private static readonly string[] WarmUpMaterials =
    [
        "KQvK", "KRvK", "KPvK", "KBBvK", "KBNvK", "KRPvK", "KQPvK", "KBPvK", "KNPvK",
        "KQvKR", "KQvKB", "KQvKN", "KQvKP", "KRvKB", "KRvKN", "KRvKP", "KRvKR", "KBvKN", "KBvKB", "KNvKN",
        "KPvKP", "KQvKQ", "KRPvKR", "KRBvKR", "KRNvKR", "KRRvKR", "KBPvKB", "KNPvKN", "KRPvKP", "KPPvK"
    ];

    /// <summary>
    /// Probes a few legal-looking positions per common material set so Fathom maps those table files
    /// and the OS page cache is hot before the first real search. Runs once, in the background.
    /// </summary>
    private void WarmUp()
    {
        try
        {
            var rnd = new Random(12345);
            foreach (var material in WarmUpMaterials)
            {
                int split = material.IndexOf('v');
                string white = material[..split];
                string black = material[(split + 1)..];
                if (white.Length + black.Length > _syzygy.LargestTable) continue;

                for (int attempt = 0; attempt < 6; attempt++)
                {
                    if (TryBuildWarmUpPosition(rnd, white, black, attempt % 2 == 0, out var position))
                        _syzygy.TryProbeWdl(position, out _);
                }
            }
        }
        catch
        {
        }
    }

    private static bool TryBuildWarmUpPosition(Random rnd, string white, string black, bool whiteToMove, out SyzygyPosition position)
    {
        ulong occupied = 0, w = 0, b = 0, kings = 0, queens = 0, rooks = 0, bishops = 0, knights = 0, pawns = 0;
        position = default;
        int whiteKing = -1;

        for (int side = 0; side < 2; side++)
        {
            foreach (char piece in side == 0 ? white : black)
            {
                int square = -1;
                for (int tries = 0; tries < 64; tries++)
                {
                    int s = rnd.Next(64);
                    if ((occupied & (1UL << s)) != 0) continue;
                    if (piece == 'P' && (s < 8 || s >= 56)) continue;
                    if (piece == 'K' && side == 1 && Math.Abs((s & 7) - (whiteKing & 7)) <= 1 && Math.Abs((s >> 3) - (whiteKing >> 3)) <= 1) continue;
                    square = s;
                    break;
                }
                if (square < 0) return false;

                ulong bit = 1UL << square;
                occupied |= bit;
                if (side == 0) w |= bit; else b |= bit;
                if (piece == 'K' && side == 0) whiteKing = square;

                switch (piece)
                {
                    case 'K': kings |= bit; break;
                    case 'Q': queens |= bit; break;
                    case 'R': rooks |= bit; break;
                    case 'B': bishops |= bit; break;
                    case 'N': knights |= bit; break;
                    default: pawns |= bit; break;
                }
            }
        }

        position = new SyzygyPosition(w, b, kings, queens, rooks, bishops, knights, pawns, 0, 0, 0, whiteToMove);
        return true;
    }

    public bool IsEnabled => _syzygy.IsInitialized;

    public int MaxPieces => _syzygy.LargestTable;

    public bool TryProbeWdl(Board board, bool whiteToMove, out TbResult wdl)
    {
        wdl = TbResult.Draw;

        if (!_syzygy.IsInitialized || board.Occupied.Count() > _syzygy.LargestTable)
            return false;

        ulong key = (whiteToMove ? board.Hash : board.Hash ^ BlackKey) & KeyMask;
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

        var position = CreatePosition(board, whiteToMove, 0);
        if (!_syzygy.TryProbeWdl(position, out wdl))
        {
            slot = key | Failed;
            return false;
        }

        slot = key | ((ulong)wdl + 1);
        return true;
    }

    public bool TryProbeRoot(Board board, bool whiteToMove, int rule50, out SyzygyRootResult root)
    {
        root = SyzygyRootResult.Invalid;

        if (!_syzygy.IsInitialized || board.Occupied.Count() > _syzygy.LargestTable)
            return false;

        ulong key = whiteToMove ? board.Hash : board.Hash ^ BlackKey;
        var cached = _root;
        if (cached != null && cached.Key == key && cached.Rule50 == (uint)rule50)
        {
            root = cached.Result;
            return cached.Found;
        }

        var result = _syzygy.ProbeRoot(CreatePosition(board, whiteToMove, (uint)rule50));
        _root = new RootEntry { Key = key, Rule50 = (uint)rule50, Found = result.IsValid, Result = result };

        root = result;
        return result.IsValid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SyzygyPosition CreatePosition(Board board, bool whiteToMove, uint rule50)
    {
        ulong knights = (ulong)board.GetPieceBits(Pieces.WhiteKnight) | (ulong)board.GetPieceBits(Pieces.BlackKnight);
        ulong bishops = (ulong)board.GetPieceBits(Pieces.WhiteBishop) | (ulong)board.GetPieceBits(Pieces.BlackBishop);
        ulong rooks = (ulong)board.GetPieceBits(Pieces.WhiteRook) | (ulong)board.GetPieceBits(Pieces.BlackRook);
        ulong queens = (ulong)board.GetPieceBits(Pieces.WhiteQueen) | (ulong)board.GetPieceBits(Pieces.BlackQueen);
        ulong kings = (ulong)board.GetPieceBits(Pieces.WhiteKing) | (ulong)board.GetPieceBits(Pieces.BlackKing);
        ulong pawns = (ulong)board.GetPieceBits(Pieces.WhitePawn) | (ulong)board.GetPieceBits(Pieces.BlackPawn);

        return new SyzygyPosition((ulong)board.Whites, (ulong)board.Blacks, kings, queens, rooks, bishops, knights, pawns,
            rule50, 0, 0, whiteToMove);
    }
}
