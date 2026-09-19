using Engine.Models.Bits;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopBattary(byte to)
    {
        ref var boardBase = ref _boards[0];
        if (Unsafe.Add(ref boardBase, Pieces.WhiteQueen).IsZero()) return false;

        var pattern = _whiteBishopPatterns[to] & _blackKingPatterns[Unsafe.Add(ref boardBase, Pieces.BlackKing).BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.WhiteQueen)) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookBattary(byte to)
    {
        ref var boardBase = ref _boards[0];
        var pattern = _whiteRookPatterns[to] & _blackKingPatterns[Unsafe.Add(ref boardBase, Pieces.BlackKing).BitScanForward()];

        return pattern.Any() && Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Any() && (to.XrayRookAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.WhiteQueen)) & pattern).Any() || (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Count() > 1 && (to.XrayRookAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.WhiteRook)) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenBattary(byte to)
    {
        ref var boardBase = ref _boards[0];
        var pattern = _whiteQueenPatterns[to] & _blackKingPatterns[Unsafe.Add(ref boardBase, Pieces.BlackKing).BitScanForward()];

        return pattern.Any() && Unsafe.Add(ref boardBase, Pieces.WhiteRook).Any() && (to.XrayRookAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.WhiteRook)) & pattern).Any() || (Unsafe.Add(ref boardBase, Pieces.WhiteBishop).Any() && (to.XrayBishopAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.WhiteBishop)) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopBattary(byte to)
    {
        ref var boardBase = ref _boards[0];
        if (Unsafe.Add(ref boardBase, Pieces.BlackQueen).IsZero()) return false;

        var pattern = _blackBishopPatterns[to] & _whiteKingPatterns[Unsafe.Add(ref boardBase, Pieces.WhiteKing).BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.BlackQueen)) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenBattary(byte to)
    {
        ref var boardBase = ref _boards[0];
        var pattern = _blackQueenPatterns[to] & _whiteKingPatterns[Unsafe.Add(ref boardBase, Pieces.WhiteKing).BitScanForward()];

        return pattern.Any() && Unsafe.Add(ref boardBase, Pieces.BlackRook).Any() && (to.XrayRookAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.BlackRook)) & pattern).Any() || (Unsafe.Add(ref boardBase, Pieces.BlackBishop).Any() && (to.XrayBishopAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.BlackBishop)) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookBattary(byte to)
    {
        ref var boardBase = ref _boards[0];
        var pattern = _blackRookPatterns[to] & _whiteKingPatterns[Unsafe.Add(ref boardBase, Pieces.WhiteKing).BitScanForward()];

        return pattern.Any() && Unsafe.Add(ref boardBase, Pieces.BlackQueen).Any() && (to.XrayRookAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.BlackQueen)) & pattern).Any() || (Unsafe.Add(ref boardBase, Pieces.BlackRook).Count() > 1 && (to.XrayRookAttacks(Occupied, Unsafe.Add(ref boardBase, Pieces.BlackRook)) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenPin(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.XrayRookAttacks(Occupied, Whites) & (Unsafe.Add(ref boardBase, Pieces.WhiteKing))).Any()
            || (to.XrayRookAttacks(Occupied, Blacks.Remove(Unsafe.Add(ref boardBase, Pieces.BlackPawn))) & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()
            || (to.XrayBishopAttacks(Occupied, Whites) & (Unsafe.Add(ref boardBase, Pieces.WhiteKing))).Any()
            || (to.XrayBishopAttacks(Occupied, Blacks.Remove(Unsafe.Add(ref boardBase, Pieces.BlackPawn))) & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenPin(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.XrayRookAttacks(Occupied, Blacks) & (Unsafe.Add(ref boardBase, Pieces.BlackKing))).Any()
            || (to.XrayRookAttacks(Occupied, Whites.Remove(Unsafe.Add(ref boardBase, Pieces.WhitePawn))) & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()
            || (to.XrayBishopAttacks(Occupied, Blacks) & (Unsafe.Add(ref boardBase, Pieces.BlackKing))).Any()
            || (to.XrayBishopAttacks(Occupied, Whites.Remove(Unsafe.Add(ref boardBase, Pieces.WhitePawn))) & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookPin(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.XrayRookAttacks(Occupied, Whites) & (Unsafe.Add(ref boardBase, Pieces.WhiteKing) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))).Any() || (to.XrayRookAttacks(Occupied, Blacks.Remove(Unsafe.Add(ref boardBase, Pieces.BlackPawn))) & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookPin(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.XrayRookAttacks(Occupied, Blacks) & (Unsafe.Add(ref boardBase, Pieces.BlackKing) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))).Any() || (to.XrayRookAttacks(Occupied, Whites.Remove(Unsafe.Add(ref boardBase, Pieces.WhitePawn))) & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopPin(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.XrayBishopAttacks(Occupied, Whites) & (Unsafe.Add(ref boardBase, Pieces.WhiteKing) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.WhiteRook))).Any() || (to.XrayBishopAttacks(Occupied, Blacks.Remove(Unsafe.Add(ref boardBase, Pieces.BlackPawn))) & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopPin(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.XrayBishopAttacks(Occupied, Blacks) & (Unsafe.Add(ref boardBase, Pieces.BlackKing) | Unsafe.Add(ref boardBase, Pieces.BlackQueen) | Unsafe.Add(ref boardBase, Pieces.BlackRook))).Any() || (to.XrayBishopAttacks(Occupied, Whites.Remove(Unsafe.Add(ref boardBase, Pieces.WhitePawn))) & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackPawnFork(byte to) => (_blackPawnPatterns[to] & Whites.Remove(Unsafe.Add(ref _boards[0], Pieces.WhitePawn))).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhitePawnFork(byte to) => (_whitePawnPatterns[to] & Blacks.Remove(Unsafe.Add(ref _boards[0], Pieces.BlackPawn))).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightFork(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (_blackKnightPatterns[to] & (Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))).Count() > 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopFork(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.BishopAttacks(Empty) & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))).Count() > 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopFork(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (to.BishopAttacks(Empty) & (Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))).Count() > 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightFork(byte to)
    {
        ref var boardBase = ref _boards[0];
        return (_whiteKnightPatterns[to] & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))).Count() > 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward()];

        return (from.BishopAttacks(Occupied) & shield).Count() < (to.BishopAttacks(Occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward()];

        return (_blackKnightPatterns[from] & shield).Count() < (_blackKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward()];

        return (from.BishopAttacks(Occupied) & shield).Count() < (to.BishopAttacks(Occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward()];

        return (_whiteKnightPatterns[from] & shield).Count() < (_whiteKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward()];

        return (from.RookAttacks(Occupied) & shield).Count() < (to.RookAttacks(Occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward()];

        return (from.RookAttacks(Occupied) & shield).Count() < (to.RookAttacks(Occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward()];

        return (from.QueenAttacks(Occupied) & shield).Count() < (to.QueenAttacks(Occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward()];

        return (from.QueenAttacks(Occupied) & shield).Count() < (to.QueenAttacks(Occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackPawn(byte to) => (_whitePawnPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.BlackPawn)).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackKnight(byte to) => (_whiteKnightPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.BlackKnight)).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackBishop(byte to) => (to.BishopAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.BlackBishop)).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteBishop(byte to) => (to.BishopAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.WhiteBishop)).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhitePawn(byte to) => (_blackPawnPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.WhitePawn)).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteKnight(byte to) => (_blackKnightPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.WhiteKnight)).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnSeven(byte from, byte to) => (_rank6 & from.AsBitBoard()).IsZero() && (_rank6 & to.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnSeven(byte from, byte to) => (_rank1 & from.AsBitBoard()).IsZero() && (_rank1 & to.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleBlackRook(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        return (from.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))).IsZero() &&
            (to.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen))).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleWhiteRook(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        return (from.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))).IsZero() &&
            (to.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen))).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnOpenFile(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        return (_rookFiles[from] & (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn))).Any() && (_rookFiles[to] & (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn))).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnOpenFile(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        return (_rookFiles[from] & (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn))).Any() && (_rookFiles[to] & (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn))).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackCandidate(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        if ((_blackFacing[from] & (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn))).Any()) return false;

        return (_blackCandidatePawnsFront[from] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count() < (_blackCandidatePawnsBack[from] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count() &&
                (_blackCandidatePawnsAttackFront[from] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count() <= (_blackCandidatePawnsAttackBack[from] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count()
                &&
                (_blackCandidatePawnsFront[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count() < (_blackCandidatePawnsBack[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count() &&
                (_blackCandidatePawnsAttackFront[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count() <= (_blackCandidatePawnsAttackBack[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteCandidate(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        if ((_whiteFacing[from] & (Unsafe.Add(ref boardBase, Pieces.WhitePawn) | Unsafe.Add(ref boardBase, Pieces.BlackPawn))).Any()) return false;

        return (_whiteCandidatePawnsFront[from] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count() < (_whiteCandidatePawnsBack[from] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count() &&
                (_whiteCandidatePawnsAttackFront[from] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count() <= (_whiteCandidatePawnsAttackBack[from] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count()
                &&
                (_whiteCandidatePawnsFront[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count() < (_whiteCandidatePawnsBack[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count() &&
                (_whiteCandidatePawnsAttackFront[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Count() <= (_whiteCandidatePawnsAttackBack[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPawnStorm(byte from)
    {
        ref var boardBase = ref _boards[0];
        return (_blackPassedPawns[from] & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any() && (_whitePassedPawns[from] & Unsafe.Add(ref boardBase, Pieces.BlackKing)).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePawnStorm(byte from)
    {
        ref var boardBase = ref _boards[0];
        return (_whitePassedPawns[from] & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any() && (_blackPassedPawns[from] & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindBlackPassed(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        if ((_blackFacing[from] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).Any())
            return false;

        BitBoard bitBoard = _blackFacing[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn);
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_blackFacing[coordinate] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).IsZero() && (_blackPassedPawns[coordinate] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindWhitePassed(byte from, byte to)
    {
        ref var boardBase = ref _boards[0];
        if ((_whiteFacing[from] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).Any())
            return false;

        BitBoard bitBoard = _whiteFacing[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn);
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_whiteFacing[coordinate] & Unsafe.Add(ref boardBase, Pieces.WhitePawn)).IsZero() && (_whitePassedPawns[coordinate] & Unsafe.Add(ref boardBase, Pieces.BlackPawn)).IsZero();
    }
}
