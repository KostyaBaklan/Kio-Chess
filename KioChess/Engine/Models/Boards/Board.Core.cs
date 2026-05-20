using Engine.DataStructures;
using Engine.Models.Bits;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using System.IO.Pipelines;
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
    public byte GetPiece(byte cell) => Unsafe.Add(ref _pieces[0], cell);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetPiece(byte cell, out byte? piece)
    {
        piece = null;
        ref var boardBase = ref _boards[0];

        foreach (var p in Enumerable.Range(0, 12))
        {
            if (!Unsafe.Add(ref boardBase, p).IsSet(cell)) continue;

            piece = (byte)p;
            break;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetWhiteKingPosition() => Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlackKingPosition() => Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PositionsList GetPiecePositions(byte index)
    {
        Unsafe.Add(ref _boards[0], index).GetPositions(_positionList);
        return _positionList;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePawnSquares() => _notRank6 & Unsafe.Add(ref _boards[0], Pieces.WhitePawn);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPawnSquares() => _notRank1 & Unsafe.Add(ref _boards[0], Pieces.BlackPawn);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetWhitePromotionSquares() => _rank6 & Unsafe.Add(ref _boards[0], Pieces.WhitePawn);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetBlackPromotionSquares() => _rank1 & Unsafe.Add(ref _boards[0], Pieces.BlackPawn);

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
    public BitBoard GetPieceBits(byte piece) => Unsafe.Add(ref _boards[0], piece);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetPerimeter() => _ranks[0] | _ranks[7] | _files[0] | _files[7];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetRank(int rank) => _ranks[rank];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard GetFile(int file) => _files[file];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPass(byte position)
    {
        // Must have no pawns (white or black) directly in front on the same file
        // AND no white pawns on adjacent files ahead that could block/capture
        ref var boardBase = ref _boards[0];
        BitBoard whitePawns = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
        return (_blackFacing[position] & (whitePawns | Unsafe.Add(ref boardBase, Pieces.BlackPawn))).IsZero()
            && (_blackPassedPawns[position] & whitePawns).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePass(byte position)
    {
        // Must have no pawns (white or black) directly in front on the same file
        // AND no black pawns on adjacent files ahead that could block/capture
        ref var boardBase = ref _boards[0];
        BitBoard blackPawns = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
        return (_whiteFacing[position] & (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | blackPawns)).IsZero()
            && (_whitePassedPawns[position] & blackPawns).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteOver(BitBoard opponentPawns) => (Unsafe.Add(ref _boards[0], Pieces.WhitePawn) & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackOver(BitBoard opponentPawns) => (Unsafe.Add(ref _boards[0], Pieces.BlackPawn) & opponentPawns).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackAttacksTo(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (_whiteKnightPatterns[to] & Unsafe.Add(ref boardBase, Pieces.BlackKnight)).Any()
            || (to.BishopAttacks(_occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))).Any()
            || (to.RookAttacks(_occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))).Any()
            || (_whitePawnPatterns[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Any()
            || (_whiteKingPatterns[to] & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteAttacksTo(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (_blackKnightPatterns[to] & Unsafe.Add(ref boardBase, Pieces.WhiteKnight)).Any()
            || (to.BishopAttacks(_occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))).Any()
            || (to.RookAttacks(_occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))).Any()
            || (_blackPawnPatterns[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Any()
            || (_blackKingPatterns[to] & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any();
    }
}
