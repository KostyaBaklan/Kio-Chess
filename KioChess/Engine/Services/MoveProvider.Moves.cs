using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public partial class MoveProvider
{
    #region Move Generation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhitePawnMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() << 8) & _board.GetEmpty();

            if (board.Any())
            {
                move = _whitePawnMoves[f][board.BitScanForward()];
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            if (_whitePawnRank2.IsSet(f))
            {
                move = _whitePawnMoves[f][(byte)(f + 16)];
                if (move.IsLegal() && _board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKnightMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteBishopMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteRookMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteQueenMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsWhiteMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetWhiteKingMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;
        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            move = _whiteKingMoves[f][position];
            if (_board.IsWhiteMoveLigal(move))
            {
                moveList.Add(move);
            }
            board = board.Remove(position);
        }

        if (f == Squares.E1)
        {
            move = _whiteKingMoves[Squares.E1][Squares.G1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.F1))
            {
                moveList.Add(move);
            }
            move = _whiteKingMoves[Squares.E1][Squares.C1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.D1))
            {
                moveList.Add(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackPawnMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;

        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() >> 8) & _board.GetEmpty();

            if (board.Any())
            {
                move = _blackPawnMoves[f][board.BitScanForward()];
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            if (_blackPawnRank7.IsSet(f))
            {
                move = _blackPawnMoves[f][(byte)(f - 16)];
                if (move.IsLegal() && _board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKnightMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackBishopMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackRookMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackQueenMoves(BitBoard squares, MoveList moveList)
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
                if (_board.IsBlackMoveLigal(move))
                {
                    moveList.Add(move);
                }
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlackKingMoves(BitBoard squares, MoveList moveList)
    {
        MoveBase move;
        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            move = _blackKingMoves[f][position];
            if (_board.IsBlackMoveLigal(move))
            {
                moveList.Add(move);
            }
            board = board.Remove(position);
        }

        if (f == Squares.E8)
        {
            move = _blackKingMoves[Squares.E8][Squares.G8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.F8))
            {
                moveList.Add(move);
            }
            move = _blackKingMoves[Squares.E8][Squares.C8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.D8))
            {
                moveList.Add(move);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase GetWhiteKingMove(byte king, byte to) => _whiteKingMoves[king][to];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveBase GetBlackKingMove(byte king, byte to) => _blackKingMoves[king][to];

    #endregion

    #region Any Moves

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhitePawnMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() << 8) & _board.GetEmpty();

            if (board.Any())
            {
                if (_board.IsWhiteMoveLigal(_whitePawnMoves[f][board.BitScanForward()]))
                    return true;
            }

            if (_whitePawnRank2.IsSet(f))
            {
                var move = _whitePawnMoves[f][(byte)(f + 16)];
                if (move.IsLegal() && _board.IsWhiteMoveLigal(move))
                    return true;
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKnightMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _whiteKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteKnightMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteBishopMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteBishopMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteRookMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteRookMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteQueenMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsWhiteMoveLigal(_whiteQueenMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyWhiteKingMoves(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _whiteKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsWhiteMoveLigal(_whiteKingMoves[f][position]))
                return true;
            board = board.Remove(position);
        }

        if (f == Squares.E1)
        {
            MoveBase move;
            move = _whiteKingMoves[Squares.E1][Squares.G1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.F1))
                return true;
            move = _whiteKingMoves[Squares.E1][Squares.C1];
            if (move.IsLegal() && _board.IsWhiteCastleLigal(move, Squares.D1))
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackPawnMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = (f.AsBitBoard() >> 8) & _board.GetEmpty();

            if (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackPawnMoves[f][position]))
                    return true;
            }

            if (_blackPawnRank7.IsSet(f))
            {
                MoveBase move;
                move = _blackPawnMoves[f][(byte)(f - 16)];
                if (move.IsLegal() && _board.IsBlackMoveLigal(move))
                    return true;
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKnightMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = _blackKnightPatterns[f] & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackKnightMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackBishopMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.BishopAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackBishopMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackRookMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.RookAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackRookMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackQueenMoves(BitBoard squares)
    {
        while (squares.Any())
        {
            var f = squares.BitScanForward();
            BitBoard board = f.QueenAttacks(_board.GetOccupied()) & _board.GetEmpty();

            while (board.Any())
            {
                byte position = board.BitScanForward();
                if (_board.IsBlackMoveLigal(_blackQueenMoves[f][position]))
                    return true;
                board = board.Remove(position);
            }

            squares = squares.Remove(f);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AnyBlackKingMoves(BitBoard squares)
    {
        var f = squares.BitScanForward();
        BitBoard board = _blackKingPatterns[f] & _board.GetEmpty();

        while (board.Any())
        {
            byte position = board.BitScanForward();
            if (_board.IsBlackMoveLigal(_blackKingMoves[f][position]))
                return true;
            board = board.Remove(position);
        }

        if (f == Squares.E8)
        {
            MoveBase move;
            move = _blackKingMoves[Squares.E8][Squares.G8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.F8))
                return true;
            move = _blackKingMoves[Squares.E8][Squares.C8];
            if (move.IsLegal() && _board.IsBlackCastleLigal(move, Squares.D8))
                return true;
        }

        return false;
    }

    #endregion
}
