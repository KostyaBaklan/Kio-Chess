using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    private BitBoard _whiteKingAttacks;
    private BitBoard _blackKingAttacks;

    private CellBuffer<BitBoard> _whiteBishopAttacks;
    private CellBuffer<BitBoard> _whiteRookAttacks;
    private CellBuffer<BitBoard> _whiteQueenAttacks;

    private CellBuffer<BitBoard> _blackBishopAttacks;
    private CellBuffer<BitBoard> _blackRookAttacks;
    private CellBuffer<BitBoard> _blackQueenAttacks;

    private void InitializeAttackBuffers()
    {
        _whiteBishopAttacks = new CellBuffer<BitBoard>();
        _whiteRookAttacks = new CellBuffer<BitBoard>();
        _whiteQueenAttacks = new CellBuffer<BitBoard>();

        _blackBishopAttacks = new CellBuffer<BitBoard>();
        _blackRookAttacks = new CellBuffer<BitBoard>();
        _blackQueenAttacks = new CellBuffer<BitBoard>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeAttacks()
    {
        ref var boardBase = ref _boards[0];
        _whiteKingPosition = Unsafe.Add(ref boardBase, Pieces.WhiteKing).BitScanForward();
        _blackKingPosition = Unsafe.Add(ref boardBase, Pieces.BlackKing).BitScanForward();
        _whiteKingZone = _whiteKingShield[_whiteKingPosition];
        _blackKingZone = _blackKingShield[_blackKingPosition];

        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();

        _whiteKingAttacks = _whiteKingPatterns[_whiteKingPosition];
        _blackKingAttacks = _blackKingPatterns[_blackKingPosition];

        ComputeWhiteBishopAttacks();
        ComputeWhiteRookAttacks();
        ComputeWhiteQueenAttacks();

        ComputeBlackBishopAttacks();
        ComputeBlackRookAttacks();
        ComputeBlackQueenAttacks();
    }

    #region Attack Computation Methods (Called once per evaluation)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePawnAttacks()
    {
        ref var boardBase = ref _boards[0];
        var whitePawns = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
        return ((whitePawns & _notFileA) << 7) | ((whitePawns & _notFileH) << 9);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnAttacks()
    {
        ref var boardBase = ref _boards[0];
        var blackPawns = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
        return ((blackPawns & _notFileA) >> 9) | ((blackPawns & _notFileH) >> 7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeWhiteBishopAttacks()
    {
        ref var boardBase = ref _boards[0];
        ulong bishops = Unsafe.Add(ref boardBase, Pieces.WhiteBishop);

        while (bishops != 0)
        {
            var position = (byte)BitOperations.TrailingZeroCount(bishops);
            _whiteBishopAttacks[position] = position.BishopAttacks(_occupied);
            bishops &= bishops - 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeBlackBishopAttacks()
    {
        ref var boardBase = ref _boards[0];
        ulong bishops = Unsafe.Add(ref boardBase, Pieces.BlackBishop);

        while (bishops != 0)
        {
            var position = (byte)BitOperations.TrailingZeroCount(bishops);
            _blackBishopAttacks[position] = position.BishopAttacks(_occupied);
            bishops &= bishops - 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeWhiteRookAttacks()
    {
        ref var boardBase = ref _boards[0];
        ulong rooks = Unsafe.Add(ref boardBase, Pieces.WhiteRook);

        while (rooks != 0)
        {
            var position = (byte)BitOperations.TrailingZeroCount(rooks);
            _whiteRookAttacks[position] = position.RookAttacks(_occupied);
            rooks &= rooks - 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeBlackRookAttacks()
    {
        ref var boardBase = ref _boards[0];
        ulong rooks = Unsafe.Add(ref boardBase, Pieces.BlackRook);

        while (rooks != 0)
        {
            var position = (byte)BitOperations.TrailingZeroCount(rooks);
            _blackRookAttacks[position] = position.RookAttacks(_occupied);
            rooks &= rooks - 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeWhiteQueenAttacks()
    {
        ref var boardBase = ref _boards[0];
        ulong queens = Unsafe.Add(ref boardBase, Pieces.WhiteQueen);

        while (queens != 0)
        {
            var position = (byte)BitOperations.TrailingZeroCount(queens);
            _whiteQueenAttacks[position] = position.QueenAttacks(_occupied);
            queens &= queens - 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeBlackQueenAttacks()
    {
        ref var boardBase = ref _boards[0];
        ulong queens = Unsafe.Add(ref boardBase, Pieces.BlackQueen);

        while (queens != 0)
        {
            var position = (byte)BitOperations.TrailingZeroCount(queens);
            _blackQueenAttacks[position] = position.QueenAttacks(_occupied);
            queens &= queens - 1;
        }
    }

    #endregion
}
