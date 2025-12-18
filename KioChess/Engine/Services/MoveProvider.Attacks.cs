using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public partial class MoveProvider
{
    #region Attack Generation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePawnSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whitePawnPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whitePawnAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            if (_blackPawnRank5.IsSet(f))
            {
                for (byte i = 0; i < _whitePawnOverAttacks[f].Count; i++)
                {
                    attack = _whitePawnOverAttacks[f][i];
                    if (to.IsOff(attack.To) && attack.IsLegal() && _board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= attack.To.AsBitBoard();
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKnightSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteKnightAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteBishopSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteBishopAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteRookSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteRookAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteQueenSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _whiteQueenAttacks[f][position];
                    if (_board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKingSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        var f = squares.BitScanForward();

        BitBoard board = _whiteKingPatterns[f] & _board.GetBlacks();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (to.IsOff(position))
            {
                attack = _whiteKingAttacks[f][position];
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                    to |= position.AsBitBoard();
                }
            }
            board = board.Remove(position);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPawnSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackPawnPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackPawnAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                for (byte i = 0; i < _blackPawnOverAttacks[f].Count; i++)
                {
                    attack = _blackPawnOverAttacks[f][i];
                    if (to.IsOff(attack.To) && attack.IsLegal() && _board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= attack.To.AsBitBoard();
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKnightSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackKnightPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackKnightAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackBishopSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackBishopAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackRookSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackRookAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackQueenSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (to.IsOff(position))
                {
                    attack = _blackQueenAttacks[f][position];

                    if (_board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                        to |= position.AsBitBoard();
                    }
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKingSingleAttacks(BitBoard squares, AttackList AttackList, ref BitBoard to)
    {
        AttackBase attack;

        var f = squares.BitScanForward();

        BitBoard board = _blackKingPatterns[f] & _board.GetWhites();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (to.IsOff(position))
            {
                attack = _blackKingAttacks[f][position];

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                    to |= position.AsBitBoard();
                }
            }
            board = board.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePawnAttacks(BitBoard squares, AttackList AttackList)
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
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            if (_blackPawnRank5.IsSet(f))
            {
                for (byte i = 0; i < _whitePawnOverAttacks[f].Count; i++)
                {
                    attack = _whitePawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsWhiteMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKnightAttacks(BitBoard squares, AttackList AttackList)
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
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteBishopAttacks(BitBoard squares, AttackList AttackList)
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
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteRookAttacks(BitBoard squares, AttackList AttackList)
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
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteQueenAttacks(BitBoard squares, AttackList AttackList)
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
                if (_board.IsWhiteMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKingAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetBlacks();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            attack = _whiteKingAttacks[f][position];
            if (_board.IsWhiteMoveLigal(attack))
            {
                AttackList.Add(attack);
            }
            board = board.Remove(position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPawnAttacks(BitBoard squares, AttackList AttackList)
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

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                for (byte i = 0; i < _blackPawnOverAttacks[f].Count; i++)
                {
                    attack = _blackPawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsBlackMoveLigal(attack))
                    {
                        AttackList.Add(attack);
                    }
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKnightAttacks(BitBoard squares, AttackList AttackList)
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

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackBishopAttacks(BitBoard squares, AttackList AttackList)
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

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackRookAttacks(BitBoard squares, AttackList AttackList)
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

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackQueenAttacks(BitBoard squares, AttackList AttackList)
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

                if (_board.IsBlackMoveLigal(attack))
                {
                    AttackList.Add(attack);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKingAttacks(BitBoard squares, AttackList AttackList)
    {
        AttackBase attack;

        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetWhites();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            attack = _blackKingAttacks[f][position];

            if (_board.IsBlackMoveLigal(attack))
            {
                AttackList.Add(attack);
            }
            board = board.Remove(position);
        }
    }

    #endregion

    #region Any Attacks

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhitePawnAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whitePawnPatterns[f] & _board.GetBlacks();
            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whitePawnAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            if (_blackPawnRank5.IsSet(f))
            {
                AttackBase attack;
                for (byte i = 0; i < _whitePawnOverAttacks[f].Count; i++)
                {
                    attack = _whitePawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsWhiteMoveLigal(attack))
                        return true;
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKnightAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteKnightAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteBishopAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteBishopAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteRookAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteRookAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteQueenAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetBlacks();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteQueenAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKingAttacks(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetBlacks();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsWhiteMoveLigal(_whiteKingAttacks[f][position]))
                return true;
            board = board.Remove(position);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackPawnAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackPawnPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackPawnAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            if (_whitePawnRank4.IsSet(f))
            {
                AttackBase attack;
                for (byte i = 0; i < _blackPawnOverAttacks[f].Count; i++)
                {
                    attack = _blackPawnOverAttacks[f][i];
                    if (attack.IsLegal() && _board.IsBlackMoveLigal(attack))
                        return true;
                }
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKnightAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = _blackKnightPatterns[f] & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();

                if (_board.IsBlackMoveLigal(_blackKnightAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackBishopAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackBishopAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackRookAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackRookAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackQueenAttacks(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();

            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetWhites();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackQueenAttacks[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKingAttacks(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetWhites();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsBlackMoveLigal(_blackKingAttacks[f][position]))
                return true;

            board = board.Remove(position);
        }

        return false;
    }

    #endregion

    #region Attack Accessors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteKingAttack(byte king, byte to) => _whiteKingAttacks[king][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackKingAttack(byte king, byte to) => _blackKingAttacks[king][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhitePawnAttacks(byte from, byte to) => _whitePawnAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteKnightAttacks(byte from, byte to) => _whiteKnightAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteBishopAttacks(byte from, byte to) => _whiteBishopAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteRookAttacks(byte from, byte to) => _whiteRookAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteQueenAttacks(byte from, byte to) => _whiteQueenAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetWhiteKingAttacks(byte from, byte to) => _whiteKingAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackPawnAttacks(byte from, byte to) => _blackPawnAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackKnightAttacks(byte from, byte to) => _blackKnightAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackBishopAttacks(byte from, byte to) => _blackBishopAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackRookAttacks(byte from, byte to) => _blackRookAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackQueenAttacks(byte from, byte to) => _blackQueenAttacks[from][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AttackBase GetBlackKingAttacks(byte from, byte to) => _blackKingAttacks[from][to];

    #endregion
}
