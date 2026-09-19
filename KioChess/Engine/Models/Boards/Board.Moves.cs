using Engine.DataStructures.Moves.Lists;
using Engine.Models.Bits;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteMoveLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsCheckToWhite();

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsWhiteNotLegal(move);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWhiteCastleLigal(MoveBase move, byte rook)
    {
        move.Make();

        bool isLegal = !IsWhiteCastleNotLegal(move.To, rook);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteNotLegal(MoveBase move) => IsBlackAttacksTo(GetWhiteKingPosition()) ||
            (move.IsCastle && IsBlackAttacksTo(move.To == Squares.C1 ? Squares.D1 : Squares.F1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteCastleNotLegal(byte king, byte rook) => IsBlackAttacksTo(king) || IsBlackAttacksTo(rook);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackMoveLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsCheckToBlack();

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackCastleLigal(MoveBase move, byte rook)
    {
        move.Make();

        bool isLegal = !IsBlackCastleNotLegal(move.To, rook);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsBlackLigal(MoveBase move)
    {
        move.Make();

        bool isLegal = !IsBlackNotLegal(move);

        move.UnMake();

        return isLegal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackCastleNotLegal(byte king, byte rook) => IsWhiteAttacksTo(king) || IsWhiteAttacksTo(rook);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackNotLegal(MoveBase move) => IsWhiteAttacksTo(GetBlackKingPosition()) ||
             (move.IsCastle && IsWhiteAttacksTo(move.To == Squares.C8 ? Squares.D8 : Squares.F8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyWhiteAttackTo(byte to) => AnyWhitePawnAttackTo(to) ||
            AnyWhiteKnightAttackTo(to) ||
            AnyWhiteBishopAttackTo(to) ||
            AnyWhiteRookAttackTo(to) ||
            AnyWhiteQueenAttackTo(to) ||
            AnyWhiteKingAttackTo(to) ||
        GetWhitePromotionsAttacksTo(to, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKingAttackTo(byte to)
    {
        byte from = Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward();
        return _whiteKingPatterns[from].IsSet(to) && IsWhiteMoveLigal(_moveProvider.GetWhiteKingAttacks(from, to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteQueenAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.WhiteQueen) & to.QueenAttacks(Occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteQueenAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteRookAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.WhiteRook) & to.RookAttacks(Occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteRookAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteBishopAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.WhiteBishop) & to.BishopAttacks(Occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteBishopAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhiteKnightAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.WhiteKnight) & _whiteKnightPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhiteKnightAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyWhitePawnAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.WhitePawn).Remove(_rank6) & _blackPawnPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsWhiteMoveLigal(_moveProvider.GetWhitePawnAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyBlackAttackTo(byte to) => AnyBlackPawnAttackTo(to) ||
            AnyBlackKnightAttackTo(to) ||
            AnyBlackBishopAttackTo(to) ||
            AnyBlackRookAttackTo(to) ||
            AnyBlackQueenAttackTo(to) ||
            AnyBlackKingAttackTo(to) ||
        GetBlackPromotionsAttacksTo(to, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKingAttackTo(byte to)
    {
        byte from = Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward();
        return _blackKingPatterns[from].IsSet(to) && IsBlackMoveLigal(_moveProvider.GetBlackKingAttacks(from, to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackQueenAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.BlackQueen) & to.QueenAttacks(Occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackQueenAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackRookAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.BlackRook) & to.RookAttacks(Occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackRookAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackBishopAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.BlackBishop) & to.BishopAttacks(Occupied);

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackBishopAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackKnightAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.BlackKnight) & _blackKnightPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackKnightAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AnyBlackPawnAttackTo(byte to)
    {
        var fromBoard = Unsafe.Add(ref _boards[0], Pieces.BlackPawn).Remove(_rank1) & _whitePawnPatterns[to];

        while (fromBoard.Any())
        {
            byte from = fromBoard.BitScanForward();
            if (IsBlackMoveLigal(_moveProvider.GetBlackPawnAttacks(from, to)))
                return true;
            fromBoard = fromBoard.Remove(from);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetWhiteAttackToForCheck(byte to) =>
        GetWhitePawnAttacksTo(to, out AttackBase attack) ||
        GetWhiteKnightAttacksTo(to, out attack) ||
        GetWhiteBishopAttacksTo(to, out attack) ||
        GetWhiteRookAttacksTo(to, out attack) ||
        GetWhiteQueenAttacksTo(to, out attack) ||
        GetWhiteKingAttacksTo(to, out attack) ||
        GetWhitePromotionsAttacksTo(to, out attack)
            ? attack
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetWhiteAttackToForPromotion(byte to) => GetWhiteKnightAttacksTo(to, out AttackBase attack) ||
        GetWhiteBishopAttacksTo(to, out attack) ||
        GetWhiteRookAttacksTo(to, out attack) ||
        GetWhiteQueenAttacksTo(to, out attack) ||
        GetWhiteKingAttacksTo(to, out attack)
            ? attack
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhitePawnAttacksTo(byte to, out AttackBase attack)
    {
        if (!_ranks[7].IsSet(to))
        {
            var attacks = _blackPawnPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.WhitePawn);

            while (attacks.Any())
            {
                byte from = attacks.BitScanForward();
                attack = _moveProvider.GetWhitePawnAttacks(from, to);
                if (IsWhiteMoveLigal(attack))
                    return true;
                attacks = attacks.Remove(from);
            }
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhitePromotionsAttacksTo(byte to, out AttackBase attack)
    {
        if (_ranks[7].IsSet(to))
        {
            var attacks = _blackPawnPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.WhitePawn);

            while (attacks.Any())
            {
                byte from = attacks.BitScanForward();
                attack = _moveProvider.GetWhitePromotionAttacks(from).FirstOrDefault(l => l[0].To == to).FirstOrDefault();
                if (IsWhiteMoveLigal(attack))
                    return true;
                attacks = attacks.Remove(from);
            }
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteKnightAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _whiteKnightPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.WhiteKnight);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteKnightAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteQueenAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.QueenAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.WhiteQueen);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteQueenAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteBishopAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.BishopAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.WhiteBishop);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteBishopAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteRookAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.RookAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.WhiteRook);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteRookAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetWhiteKingAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _whiteKingPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.WhiteKing);

        if (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetWhiteKingAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                return true;
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetBlackAttackToForCheck(byte to) =>
        GetBlackPawnAttacksTo(to, out AttackBase attack) ||
        GetBlackKnightAttacksTo(to, out attack) ||
        GetBlackBishopAttacksTo(to, out attack) ||
        GetBlackRookAttacksTo(to, out attack) ||
        GetBlackQueenAttacksTo(to, out attack) ||
        GetBlackKingAttacksTo(to, out attack) ||
        GetBlackPromotionsAttacksTo(to, out attack)
            ? attack
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetBlackAttackToForPromotion(byte to) => GetBlackKnightAttacksTo(to, out AttackBase attack) ||
        GetBlackBishopAttacksTo(to, out attack) ||
        GetBlackRookAttacksTo(to, out attack) ||
        GetBlackQueenAttacksTo(to, out attack) ||
        GetBlackKingAttacksTo(to, out attack)
            ? attack
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackPawnAttacksTo(byte to, out AttackBase attack)
    {
        if (!_ranks[0].IsSet(to))
        {
            var attacks = _whitePawnPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.BlackPawn);

            while (attacks.Any())
            {
                byte from = attacks.BitScanForward();
                attack = _moveProvider.GetBlackPawnAttacks(from, to);
                if (IsBlackMoveLigal(attack))
                    return true;
                attacks = attacks.Remove(from);
            }
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackPromotionsAttacksTo(byte to, out AttackBase attack)
    {
        if (_ranks[0].IsSet(to))
        {
            var attacks = _whitePawnPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.BlackPawn);

            while (attacks.Any())
            {
                byte from = attacks.BitScanForward();
                attack = _moveProvider.GetBlackPromotionAttacks(from).FirstOrDefault(l => l[0].To == to).FirstOrDefault();
                if (IsBlackMoveLigal(attack))
                    return true;
                attacks = attacks.Remove(from);
            }
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackKnightAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _blackKnightPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.BlackKnight);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackKnightAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackQueenAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.QueenAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.BlackQueen);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackQueenAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackBishopAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.BishopAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.BlackBishop);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackBishopAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackRookAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = to.RookAttacks(Occupied) & Unsafe.Add(ref _boards[0], Pieces.BlackRook);

        while (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackRookAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
            attacks = attacks.Remove(from);
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBlackKingAttacksTo(byte to, out AttackBase attack)
    {
        var attacks = _blackKingPatterns[to] & Unsafe.Add(ref _boards[0], Pieces.BlackKing);

        if (attacks.Any())
        {
            byte from = attacks.BitScanForward();
            attack = _moveProvider.GetBlackKingAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                return true;
        }

        attack = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal BitBoard GetWhiteKingAttackPositions()
    {
        ref var boardBase = ref _boards[0];
        var king = Unsafe.Add(ref boardBase, Pieces.WhiteKing).BitScanForward();
        BitBoard attacks = new BitBoard();

        attacks |= _whitePawnPatterns[king] & Unsafe.Add(ref boardBase, Pieces.BlackPawn);
        attacks |= _whiteKnightPatterns[king] & Unsafe.Add(ref boardBase, Pieces.BlackKnight);
        attacks |= king.BishopAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackBishop) | Unsafe.Add(ref boardBase, Pieces.BlackQueen));
        attacks |= king.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen));

        return attacks;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal BitBoard GetBlackKingAttackPositions()
    {
        ref var boardBase = ref _boards[0];
        var king = Unsafe.Add(ref boardBase, Pieces.BlackKing).BitScanForward();
        BitBoard attacks = new BitBoard();

        attacks |= _blackPawnPatterns[king] & Unsafe.Add(ref boardBase, Pieces.WhitePawn);
        attacks |= _blackKnightPatterns[king] & Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
        attacks |= king.BishopAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteBishop) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen));
        attacks |= king.RookAttacks(Occupied) & (Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen));

        return attacks;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyWhiteKingMovesOnCheck()
    {
        var king = Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward();
        var moves = _whiteKingPatterns[king] & Empty;
        while (moves.Any())
        {
            byte to = moves.BitScanForward();
            var move = _moveProvider.GetWhiteKingMove(king, to);
            if (IsWhiteMoveLigal(move))
                return true;
            moves = moves.Remove(to);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyBlackKingMovesOnCheck()
    {
        var king = Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward();
        var moves = _blackKingPatterns[king] & Empty;
        while (moves.Any())
        {
            byte to = moves.BitScanForward();
            var move = _moveProvider.GetBlackKingMove(king, to);
            if (IsBlackMoveLigal(move))
                return true;
            moves = moves.Remove(to);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyWhiteKingAttacksOnCheck()
    {
        var king = Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward();
        var moves = _whiteKingPatterns[king] & Blacks;
        while (moves.Any())
        {
            byte to = moves.BitScanForward();
            var move = _moveProvider.GetWhiteKingAttack(king, to);
            if (IsWhiteMoveLigal(move))
                return true;
            moves = moves.Remove(to);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetBlackKingAttacksOnCheck()
    {
        var king = Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward();
        var moves = _blackKingPatterns[king] & Whites;
        while (moves.Any())
        {
            byte to = moves.BitScanForward();
            var move = _moveProvider.GetBlackKingAttack(king, to);
            if (IsBlackMoveLigal(move))
                return move;
            moves = moves.Remove(to);
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AttackBase GetWhiteKingAttacksOnCheck()
    {
        var king = Unsafe.Add(ref _boards[0], Pieces.WhiteKing).BitScanForward();
        var moves = _whiteKingPatterns[king] & Blacks;
        while (moves.Any())
        {
            byte to = moves.BitScanForward();
            var move = _moveProvider.GetWhiteKingAttack(king, to);
            if (IsWhiteMoveLigal(move))
                return move;
            moves = moves.Remove(to);
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AnyBlackKingAttacksOnCheck()
    {
        var king = Unsafe.Add(ref _boards[0], Pieces.BlackKing).BitScanForward();
        var moves = _blackKingPatterns[king] & Whites;
        while (moves.Any())
        {
            byte to = moves.BitScanForward();
            var move = _moveProvider.GetBlackKingAttack(king, to);
            if (IsBlackMoveLigal(move))
                return true;
            moves = moves.Remove(to);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateWhiteAttacks(AttackList attacks)
    {
        _moveProvider.GetWhitePawnAttacks(GetWhitePawnSquares(), attacks);
        _moveProvider.GetWhiteKnightAttacks(GetPieceBits(Pieces.WhiteKnight), attacks);
        _moveProvider.GetWhiteBishopAttacks(GetPieceBits(Pieces.WhiteBishop), attacks);
        _moveProvider.GetWhiteRookAttacks(GetPieceBits(Pieces.WhiteRook), attacks);
        _moveProvider.GetWhiteQueenAttacks(GetPieceBits(Pieces.WhiteQueen), attacks);
        _moveProvider.GetWhiteKingAttacks(GetPieceBits(Pieces.WhiteKing), attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateBlackAttacks(AttackList attacks)
    {
        _moveProvider.GetBlackPawnAttacks(GetBlackPawnSquares(), attacks);
        _moveProvider.GetBlackKnightAttacks(GetPieceBits(Pieces.BlackKnight), attacks);
        _moveProvider.GetBlackBishopAttacks(GetPieceBits(Pieces.BlackBishop), attacks);
        _moveProvider.GetBlackRookAttacks(GetPieceBits(Pieces.BlackRook), attacks);
        _moveProvider.GetBlackQueenAttacks(GetPieceBits(Pieces.BlackQueen), attacks);
        _moveProvider.GetBlackKingAttacks(GetPieceBits(Pieces.BlackKing), attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateWhiteAttacksTo(byte to, AttackList attacks)
    {
        ref var boardBase = ref _boards[0];
        
        // White pawns attacking to 'to' (excluding promotion rank)
        if (!_ranks[7].IsSet(to))
        {
            var pawnAttackers = _blackPawnPatterns[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn).Remove(_rank6);
            while (pawnAttackers.Any())
            {
                byte from = pawnAttackers.BitScanForward();
                var attack = _moveProvider.GetWhitePawnAttacks(from, to);
                if (IsWhiteMoveLigal(attack))
                    attacks.Add(attack);
                pawnAttackers = pawnAttackers.Remove(from);
            }
        }

        // White knight attacks to 'to'
        var knightAttackers = _whiteKnightPatterns[to] & Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
        while (knightAttackers.Any())
        {
            byte from = knightAttackers.BitScanForward();
            var attack = _moveProvider.GetWhiteKnightAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                attacks.Add(attack);
            knightAttackers = knightAttackers.Remove(from);
        }

        // White bishop attacks to 'to'
        var bishopAttackers = to.BishopAttacks(Occupied) & Unsafe.Add(ref boardBase, Pieces.WhiteBishop);
        while (bishopAttackers.Any())
        {
            byte from = bishopAttackers.BitScanForward();
            var attack = _moveProvider.GetWhiteBishopAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                attacks.Add(attack);
            bishopAttackers = bishopAttackers.Remove(from);
        }

        // White rook attacks to 'to'
        var rookAttackers = to.RookAttacks(Occupied) & Unsafe.Add(ref boardBase, Pieces.WhiteRook);
        while (rookAttackers.Any())
        {
            byte from = rookAttackers.BitScanForward();
            var attack = _moveProvider.GetWhiteRookAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                attacks.Add(attack);
            rookAttackers = rookAttackers.Remove(from);
        }

        // White queen attacks to 'to'
        var queenAttackers = to.QueenAttacks(Occupied) & Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        while (queenAttackers.Any())
        {
            byte from = queenAttackers.BitScanForward();
            var attack = _moveProvider.GetWhiteQueenAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                attacks.Add(attack);
            queenAttackers = queenAttackers.Remove(from);
        }

        // White king attacks to 'to'
        var kingAttackers = _whiteKingPatterns[to] & Unsafe.Add(ref boardBase, Pieces.WhiteKing);
        if (kingAttackers.Any())
        {
            byte from = kingAttackers.BitScanForward();
            var attack = _moveProvider.GetWhiteKingAttacks(from, to);
            if (IsWhiteMoveLigal(attack))
                attacks.Add(attack);
        }

        // White promotion attacks to 'to'
        if (_ranks[7].IsSet(to))
        {
            var promotionAttackers = _blackPawnPatterns[to] & Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            while (promotionAttackers.Any())
            {
                byte from = promotionAttackers.BitScanForward();
                var promotions = _moveProvider.GetWhitePromotionAttacks(from);
                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count > 0 && promotions[i][0].To == to && IsWhiteMoveLigal(promotions[i][0]))
                        attacks.Add(promotions[i][0]);
                }
                promotionAttackers = promotionAttackers.Remove(from);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateBlackAttacksTo(byte to, AttackList attacks)
    {
        ref var boardBase = ref _boards[0];
        
        // Black pawns attacking to 'to' (excluding promotion rank)
        if (!_ranks[0].IsSet(to))
        {
            var pawnAttackers = _whitePawnPatterns[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn).Remove(_rank1);
            while (pawnAttackers.Any())
            {
                byte from = pawnAttackers.BitScanForward();
                var attack = _moveProvider.GetBlackPawnAttacks(from, to);
                if (IsBlackMoveLigal(attack))
                    attacks.Add(attack);
                pawnAttackers = pawnAttackers.Remove(from);
            }
        }

        // Black knight attacks to 'to'
        var knightAttackers = _blackKnightPatterns[to] & Unsafe.Add(ref boardBase, Pieces.BlackKnight);
        while (knightAttackers.Any())
        {
            byte from = knightAttackers.BitScanForward();
            var attack = _moveProvider.GetBlackKnightAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                attacks.Add(attack);
            knightAttackers = knightAttackers.Remove(from);
        }

        // Black bishop attacks to 'to'
        var bishopAttackers = to.BishopAttacks(Occupied) & Unsafe.Add(ref boardBase, Pieces.BlackBishop);
        while (bishopAttackers.Any())
        {
            byte from = bishopAttackers.BitScanForward();
            var attack = _moveProvider.GetBlackBishopAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                attacks.Add(attack);
            bishopAttackers = bishopAttackers.Remove(from);
        }

        // Black rook attacks to 'to'
        var rookAttackers = to.RookAttacks(Occupied) & Unsafe.Add(ref boardBase, Pieces.BlackRook);
        while (rookAttackers.Any())
        {
            byte from = rookAttackers.BitScanForward();
            var attack = _moveProvider.GetBlackRookAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                attacks.Add(attack);
            rookAttackers = rookAttackers.Remove(from);
        }

        // Black queen attacks to 'to'
        var queenAttackers = to.QueenAttacks(Occupied) & Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        while (queenAttackers.Any())
        {
            byte from = queenAttackers.BitScanForward();
            var attack = _moveProvider.GetBlackQueenAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                attacks.Add(attack);
            queenAttackers = queenAttackers.Remove(from);
        }

        // Black king attacks to 'to'
        var kingAttackers = _blackKingPatterns[to] & Unsafe.Add(ref boardBase, Pieces.BlackKing);
        if (kingAttackers.Any())
        {
            byte from = kingAttackers.BitScanForward();
            var attack = _moveProvider.GetBlackKingAttacks(from, to);
            if (IsBlackMoveLigal(attack))
                attacks.Add(attack);
        }

        // Black promotion attacks to 'to'
        if (_ranks[0].IsSet(to))
        {
            var promotionAttackers = _whitePawnPatterns[to] & Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            while (promotionAttackers.Any())
            {
                byte from = promotionAttackers.BitScanForward();
                var promotions = _moveProvider.GetBlackPromotionAttacks(from);
                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count > 0 && promotions[i][0].To == to && IsBlackMoveLigal(promotions[i][0]))
                        attacks.Add(promotions[i][0]);
                }
                promotionAttackers = promotionAttackers.Remove(from);
            }
        }
    }
}