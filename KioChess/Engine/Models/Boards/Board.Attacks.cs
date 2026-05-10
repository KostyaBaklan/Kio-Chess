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
        _whiteKingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        _blackKingPosition = _boards[Pieces.BlackKing].BitScanForward();
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
    public BitBoard GetWhitePawnAttacks() => ((_boards[Pieces.WhitePawn] & _notFileA) << 7) |
               ((_boards[Pieces.WhitePawn] & _notFileH) << 9);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnAttacks() => ((_boards[Pieces.BlackPawn] & _notFileA) >> 9) |
               ((_boards[Pieces.BlackPawn] & _notFileH) >> 7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeWhiteBishopAttacks()
    {
        ulong bishops = _boards[Pieces.WhiteBishop];

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
        ulong bishops = _boards[Pieces.BlackBishop];

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
        ulong rooks = _boards[Pieces.WhiteRook];

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
        ulong rooks = _boards[Pieces.BlackRook];

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
        ulong queens = _boards[Pieces.WhiteQueen];

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
        ulong queens = _boards[Pieces.BlackQueen];

        while (queens != 0)
        {
            var position = (byte)BitOperations.TrailingZeroCount(queens);
            _blackQueenAttacks[position] = position.QueenAttacks(_occupied);
            queens &= queens - 1;
        }
    }

    #endregion
}
