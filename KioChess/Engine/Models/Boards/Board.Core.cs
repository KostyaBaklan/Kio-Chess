using Engine.DataStructures;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty(BitBoard bitBoard) => _empty.IsSet(bitBoard);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteOpposite(byte square) => _blacks.IsSet(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackOpposite(byte square) => _whites.IsSet(square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetPiece(byte cell) => _pieces[cell];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetPiece(byte cell, out byte? piece)
    {
        piece = null;

        foreach (var p in Enumerable.Range(0, 12))
        {
            if (!_boards[p].IsSet(cell)) continue;

            piece = (byte)p;
            break;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetWhiteKingPosition() => _boards[Pieces.WhiteKing].BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlackKingPosition() => _boards[Pieces.BlackKing].BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PositionsList GetPiecePositions(byte index)
    {
        _boards[index].GetPositions(_positionList);
        return _positionList;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePawnSquares() => _notRank6 & _boards[Pieces.WhitePawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnSquares() => _notRank1 & _boards[Pieces.BlackPawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePromotionSquares() => _rank6 & _boards[Pieces.WhitePawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPromotionSquares() => _rank1 & _boards[Pieces.BlackPawn];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetKey() => _hash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetOccupied() => _occupied;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetEmpty() => _empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlacks() => _blacks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhites() => _whites;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetPieceBits(byte piece) => _boards[piece];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetPerimeter() => _ranks[0] | _ranks[7] | _files[0] | _files[7];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePawnAttacks() => ((_boards[Pieces.WhitePawn] & _notFileA) << 7) |
               ((_boards[Pieces.WhitePawn] & _notFileH) << 9);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnAttacks() => ((_boards[Pieces.BlackPawn] & _notFileA) >> 9) |
               ((_boards[Pieces.BlackPawn] & _notFileH) >> 7);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetRank(int rank) => _ranks[rank];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetFile(int file) => _files[file];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPass(byte position)
    {
        // Must have no pawns (white or black) directly in front on the same file
        // AND no white pawns on adjacent files ahead that could block/capture
        BitBoard whitePawns = _boards[Pieces.WhitePawn];
        return (_blackFacing[position] & (whitePawns | _boards[Pieces.BlackPawn])).IsZero()
            && (_blackPassedPawns[position] & whitePawns).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePass(byte position)
    {
        // Must have no pawns (white or black) directly in front on the same file
        // AND no black pawns on adjacent files ahead that could block/capture
        BitBoard blackPawns = _boards[Pieces.BlackPawn];
        return (_whiteFacing[position] & (_boards[Pieces.WhitePawn] | blackPawns)).IsZero()
            && (_whitePassedPawns[position] & blackPawns).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteOver(BitBoard opponentPawns) => (_boards[Pieces.WhitePawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackOver(BitBoard opponentPawns) => (_boards[Pieces.BlackPawn] & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackAttacksTo(byte to) => (_whiteKnightPatterns[to] & _boards[Pieces.BlackKnight]).Any()
            || (to.BishopAttacks(_occupied) & (_boards[Pieces.BlackBishop] | _boards[Pieces.BlackQueen])).Any()
            || (to.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Any()
            || (_whitePawnPatterns[to] & _boards[Pieces.BlackPawn]).Any()
            || (_whiteKingPatterns[to] & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteAttacksTo(byte to) => (_blackKnightPatterns[to] & _boards[Pieces.WhiteKnight]).Any()
        || (to.BishopAttacks(_occupied) & (_boards[Pieces.WhiteBishop] | _boards[Pieces.WhiteQueen])).Any()
        || (to.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Any()
        || (_blackPawnPatterns[to] & _boards[Pieces.WhitePawn]).Any()
        || (_blackKingPatterns[to] & _boards[Pieces.WhiteKing]).Any();
}
