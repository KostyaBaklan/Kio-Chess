using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public partial class MoveProvider
{
    #region Successfull Attacks

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhitePawnAttacks(BitBoard squares)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _whitePawnPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whitePawnAttacks[f][position];

                if (_board.IsWhiteMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                for (byte i = 0; i < _whitePawnOverAttacks[f].Count; i++)
                {
                    attack = _whitePawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                    {
                        return true;
                    }
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteKnightAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _whiteKnightPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteKnightAttacks[f][position];

                if (_board.IsWhiteMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteBishopAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteBishopAttacks[f][position];

                if (_board.IsWhiteMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteRookAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteRookAttacks[f][position];

                if (_board.IsWhiteMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteQueenAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _whiteQueenAttacks[f][position];

                if (_board.IsWhiteMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteKingAttacks(BitBoard squares)
    {
        AttackBase attack;

        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetBlacks();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            attack = _whiteKingAttacks[f][position];

            if (_board.IsWhiteMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
            {
                return true;
            }
            board = board.Remove(position);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackPawnAttacks(BitBoard squares)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackPawnPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackPawnAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                for (byte i = 0; i < _blackPawnOverAttacks[f].Count; i++)
                {
                    attack = _blackPawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                    {
                        return true;
                    }
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackKnightAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackKnightPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackKnightAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackBishopAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackBishopAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackRookAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackRookAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackQueenAttacks(BitBoard squares)
    {
        AttackBase attack;
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                attack = _blackQueenAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackKingAttacks(BitBoard squares)
    {
        AttackBase attack;

        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetWhites();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            attack = _blackKingAttacks[f][position];

            if (_board.IsBlackMoveLigal(attack) && _board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
            {
                return true;
            }
            board = board.Remove(position);
        }

        return false;
    }

    #endregion

    #region Successfull Moves

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhitePawnMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() << 8) & _board.GetEmpty();

            if (board.Any())
            {
                move = _whitePawnMoves[f][board.BitScanForward()];
                if (_board.IsWhiteMoveLigal(move) && condition(move))
                {
                    return true;
                }
            }

            if (_whitePawnRank2.IsSet(f))
            {
                move = _whitePawnMoves[f][f + 16];
                if (move.IsLegal() && _board.IsWhiteMoveLigal(move) && condition(move))
                {
                    return true;
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteKnightMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteKnightMoves[f][position];
                if (_board.IsWhiteMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteBishopMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteBishopMoves[f][position];
                if (_board.IsWhiteMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteRookMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteRookMoves[f][position];
                if (_board.IsWhiteMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteQueenMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _whiteQueenMoves[f][position];
                if (_board.IsWhiteMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullWhiteKingMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;
        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            move = _whiteKingMoves[f][position];
            if (_board.IsWhiteMoveLigal(move) && condition(move))
            {
                return true;
            }
            board = board.Remove(position);
        }

        if (f == Squares.E1)
        {
            move = _whiteKingMoves[Squares.E1][Squares.G1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.F1) && condition(move))
            {
                return true;
            }
            move = _whiteKingMoves[Squares.E1][Squares.C1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.D1) && condition(move))
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackPawnMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() >> 8) & _board.GetEmpty();

            if (board.Any())
            {
                move = _blackPawnMoves[f][board.BitScanForward()];
                if (_board.IsBlackMoveLigal(move) && condition(move))
                {
                    return true;
                }
            }

            if (_blackPawnRank7.IsSet(f))
            {
                move = _blackPawnMoves[f][f - 16];
                if (move.IsLegal() && _board.IsBlackMoveLigal(move) && condition(move))
                {
                    return true;
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackKnightMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackKnightMoves[f][position];
                if (_board.IsBlackMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackBishopMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackBishopMoves[f][position];
                if (_board.IsBlackMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackRookMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackRookMoves[f][position];
                if (_board.IsBlackMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackQueenMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                move = _blackQueenMoves[f][position];
                if (_board.IsBlackMoveLigal(move) && condition(move))
                {
                    return true;
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnySuccessfullBlackKingMoves(BitBoard squares, Func<MoveBase, bool> condition)
    {
        MoveBase move;
        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            move = _blackKingMoves[f][position];
            if (_board.IsBlackMoveLigal(move) && condition(move))
            {
                return true;
            }
            board = board.Remove(position);
        }

        if (f == Squares.E8)
        {
            move = _blackKingMoves[Squares.E8][Squares.G8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.F8) && condition(move))
            {
                return true;
            }
            move = _blackKingMoves[Squares.E8][Squares.C8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.D8) && condition(move))
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}
