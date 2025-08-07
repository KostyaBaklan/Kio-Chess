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
        if (_boards[Pieces.WhiteQueen].IsZero()) return false;

        var pattern = _whiteBishopPatterns[to] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookBattary(byte to)
    {
        var pattern = _whiteRookPatterns[to] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.WhiteQueen].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any() || (_boards[Pieces.WhiteRook].Count() > 1 && (to.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenBattary(byte to)
    {
        var pattern = _whiteQueenPatterns[to] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.WhiteRook].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any() || (_boards[Pieces.WhiteBishop].Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteBishop]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopBattary(byte to)
    {
        if (_boards[Pieces.BlackQueen].IsZero()) return false;

        var pattern = _blackBishopPatterns[to] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        return pattern.Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenBattary(byte to)
    {
        var pattern = _blackQueenPatterns[to] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.BlackRook].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any() || (_boards[Pieces.BlackBishop].Any() && (to.XrayBishopAttacks(_occupied, _boards[Pieces.BlackBishop]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookBattary(byte to)
    {
        var pattern = _blackRookPatterns[to] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        return pattern.Any() && _boards[Pieces.BlackQueen].Any() && (to.XrayRookAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any() || (_boards[Pieces.BlackRook].Count() > 1 && (to.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackQueenPin(byte to) => (to.XrayRookAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing])).Any()
            || (to.XrayRookAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any()
            || (to.XrayBishopAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing])).Any()
            || (to.XrayBishopAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteQueenPin(byte to) => (to.XrayRookAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing])).Any()
            || (to.XrayRookAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any()
            || (to.XrayBishopAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing])).Any()
            || (to.XrayBishopAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackRookPin(byte to) => (to.XrayRookAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing] | _boards[Pieces.WhiteQueen])).Any() || (to.XrayRookAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteRookPin(byte to) => (to.XrayRookAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing] | _boards[Pieces.BlackQueen])).Any() || (to.XrayRookAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopPin(byte to) => (to.XrayBishopAttacks(_occupied, _whites) & (_boards[Pieces.WhiteKing] | _boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook])).Any() || (to.XrayBishopAttacks(_occupied, _blacks.Remove(_boards[Pieces.BlackPawn])) & _boards[Pieces.WhiteKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopPin(byte to) => (to.XrayBishopAttacks(_occupied, _blacks) & (_boards[Pieces.BlackKing] | _boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook])).Any() || (to.XrayBishopAttacks(_occupied, _whites.Remove(_boards[Pieces.WhitePawn])) & _boards[Pieces.BlackKing]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackPawnFork(byte to) => (_blackPawnPatterns[to] & _whites.Remove(_boards[Pieces.WhitePawn])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhitePawnFork(byte to) => (_whitePawnPatterns[to] & _blacks.Remove(_boards[Pieces.BlackPawn])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightFork(byte to) => (_blackKnightPatterns[to] & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopFork(byte to) => (to.BishopAttacks(_empty) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopFork(byte to) => (to.BishopAttacks(_empty) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightFork(byte to) => (_whiteKnightPatterns[to] & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Count() > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (from.BishopAttacks(_occupied) & shield).Count() < (to.BishopAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (_blackKnightPatterns[from] & shield).Count() < (_blackKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteBishopAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (from.BishopAttacks(_occupied) & shield).Count() < (to.BishopAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteKnightAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (_whiteKnightPatterns[from] & shield).Count() < (_whiteKnightPatterns[to] & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (from.RookAttacks(_occupied) & shield).Count() < (to.RookAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (from.RookAttacks(_occupied) & shield).Count() < (to.RookAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _blackKingShield[_boards[Pieces.BlackKing].BitScanForward()];

        return (from.QueenAttacks(_occupied) & shield).Count() < (to.QueenAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackQueenAttacksKingZone(byte from, byte to)
    {
        var shield = _whiteKingShield[_boards[Pieces.WhiteKing].BitScanForward()];

        return (from.QueenAttacks(_occupied) & shield).Count() < (to.QueenAttacks(_occupied) & shield).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackPawn(byte to) => (_whitePawnPatterns[to] & _boards[Pieces.BlackPawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackKnight(byte to) => (_whiteKnightPatterns[to] & _boards[Pieces.BlackKnight]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByBlackBishop(byte to) => (to.BishopAttacks(_occupied) & _boards[Pieces.BlackBishop]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteBishop(byte to) => (to.BishopAttacks(_occupied) & _boards[Pieces.WhiteBishop]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhitePawn(byte to) => (_blackPawnPatterns[to] & _boards[Pieces.WhitePawn]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttackedByWhiteKnight(byte to) => (_blackKnightPatterns[to] & _boards[Pieces.WhiteKnight]).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnSeven(byte from, byte to) => (_rank6 & from.AsBitBoard()).IsZero() && (_rank6 & to.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnSeven(byte from, byte to) => (_rank1 & from.AsBitBoard()).IsZero() && (_rank1 & to.AsBitBoard()).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleBlackRook(byte from, byte to) => (from.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).IsZero() &&
            (to.RookAttacks(_occupied) & (_boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDoubleWhiteRook(byte from, byte to) => (from.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).IsZero() &&
            (to.RookAttacks(_occupied) & (_boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen])).Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackRookOnOpenFile(byte from, byte to) => (_rookFiles[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any() && (_rookFiles[to] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteRookOnOpenFile(byte from, byte to) => (_rookFiles[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any() && (_rookFiles[to] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackCandidate(byte from, byte to)
    {
        if ((_blackFacing[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any()) return false;

        return (_blackCandidatePawnsFront[from] & _boards[Pieces.WhitePawn]).Count() < (_blackCandidatePawnsBack[from] & _boards[Pieces.BlackPawn]).Count() &&
                (_blackCandidatePawnsAttackFront[from] & _boards[Pieces.WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[from] & _boards[Pieces.BlackPawn]).Count()
                &&
                (_blackCandidatePawnsFront[to] & _boards[Pieces.WhitePawn]).Count() < (_blackCandidatePawnsBack[to] & _boards[Pieces.BlackPawn]).Count() &&
                (_blackCandidatePawnsAttackFront[to] & _boards[Pieces.WhitePawn]).Count() <= (_blackCandidatePawnsAttackBack[to] & _boards[Pieces.BlackPawn]).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhiteCandidate(byte from, byte to)
    {
        if ((_whiteFacing[from] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).Any()) return false;

        return (_whiteCandidatePawnsFront[from] & _boards[Pieces.BlackPawn]).Count() < (_whiteCandidatePawnsBack[from] & _boards[Pieces.WhitePawn]).Count() &&
                (_whiteCandidatePawnsAttackFront[from] & _boards[Pieces.BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[from] & _boards[Pieces.WhitePawn]).Count()
                &&
                (_whiteCandidatePawnsFront[to] & _boards[Pieces.BlackPawn]).Count() < (_whiteCandidatePawnsBack[to] & _boards[Pieces.WhitePawn]).Count() &&
                (_whiteCandidatePawnsAttackFront[to] & _boards[Pieces.BlackPawn]).Count() <= (_whiteCandidatePawnsAttackBack[to] & _boards[Pieces.WhitePawn]).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlackPawnStorm(byte from) => (_blackPassedPawns[from] & _boards[Pieces.WhiteKing]).Any() && (_whitePassedPawns[from] & _boards[Pieces.BlackKing]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsWhitePawnStorm(byte from) => (_whitePassedPawns[from] & _boards[Pieces.BlackKing]).Any() && (_blackPassedPawns[from] & _boards[Pieces.WhiteKing]).IsZero();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindBlackPassed(byte from, byte to)
    {
        if ((_blackFacing[from] & _boards[Pieces.BlackPawn]).Any())
            return false;

        BitBoard bitBoard = _blackFacing[to] & _boards[Pieces.BlackPawn];
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_blackFacing[coordinate] & _boards[Pieces.BlackPawn]).IsZero() && (_blackPassedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBehindWhitePassed(byte from, byte to)
    {
        if ((_whiteFacing[from] & _boards[Pieces.WhitePawn]).Any())
            return false;

        BitBoard bitBoard = _whiteFacing[to] & _boards[Pieces.WhitePawn];
        if (bitBoard.IsZero())
            return false;

        var coordinate = bitBoard.BitScanForward();

        return (_whiteFacing[coordinate] & _boards[Pieces.WhitePawn]).IsZero() && (_whitePassedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero();
    }
}
