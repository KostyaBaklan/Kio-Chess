using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoWhiteSmallCastle()
    {
        _pieces[Squares.G1] = Pieces.WhiteKing;
        _pieces[Squares.F1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.H1][Pieces.WhiteRook] ^ _hashTable[Squares.F1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.E1][Pieces.WhiteKing] ^ _hashTable[Squares.G1][Pieces.WhiteKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.WhiteKing) ^= _whiteSmallCastleKing;
        Unsafe.Add(ref boardBase, Pieces.WhiteRook) ^= _whiteSmallCastleRook;

        _whites ^= _whiteSmallCastleKing;
        _whites ^= _whiteSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoBlackSmallCastle()
    {
        _pieces[Squares.G8] = Pieces.BlackKing;
        _pieces[Squares.F8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.H8][Pieces.BlackRook] ^ _hashTable[Squares.F8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.E8][Pieces.BlackKing] ^ _hashTable[Squares.G8][Pieces.BlackKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.BlackKing) ^= _blackSmallCastleKing;
        Unsafe.Add(ref boardBase, Pieces.BlackRook) ^= _blackSmallCastleRook;

        _blacks ^= _blackSmallCastleKing;
        _blacks ^= _blackSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoBlackBigCastle()
    {
        _pieces[Squares.C8] = Pieces.BlackKing;
        _pieces[Squares.D8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.A8][Pieces.BlackRook] ^ _hashTable[Squares.D8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.E8][Pieces.BlackKing] ^ _hashTable[Squares.C8][Pieces.BlackKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.BlackKing) ^= _blackBigCastleKing;
        Unsafe.Add(ref boardBase, Pieces.BlackRook) ^= _blackBigCastleRook;

        _blacks ^= _blackBigCastleKing;
        _blacks ^= _blackBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoWhiteBigCastle()
    {
        _pieces[Squares.C1] = Pieces.WhiteKing;
        _pieces[Squares.D1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.A1][Pieces.WhiteRook] ^ _hashTable[Squares.D1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.E1][Pieces.WhiteKing] ^ _hashTable[Squares.C1][Pieces.WhiteKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.WhiteKing) ^= _whiteBigCastleKing;
        Unsafe.Add(ref boardBase, Pieces.WhiteRook) ^= _whiteBigCastleRook;

        _whites ^= _whiteBigCastleKing;
        _whites ^= _whiteBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoWhiteSmallCastle()
    {
        _pieces[Squares.E1] = Pieces.WhiteKing;
        _pieces[Squares.H1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.F1][Pieces.WhiteRook] ^ _hashTable[Squares.H1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.G1][Pieces.WhiteKing] ^ _hashTable[Squares.E1][Pieces.WhiteKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.WhiteKing) ^= _whiteSmallCastleKing;
        Unsafe.Add(ref boardBase, Pieces.WhiteRook) ^= _whiteSmallCastleRook;

        _whites ^= _whiteSmallCastleKing;
        _whites ^= _whiteSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoBlackSmallCastle()
    {
        _pieces[Squares.E8] = Pieces.BlackKing;
        _pieces[Squares.H8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.F8][Pieces.BlackRook] ^ _hashTable[Squares.H8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.G8][Pieces.BlackKing] ^ _hashTable[Squares.E8][Pieces.BlackKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.BlackKing) ^= _blackSmallCastleKing;
        Unsafe.Add(ref boardBase, Pieces.BlackRook) ^= _blackSmallCastleRook;

        _blacks ^= _blackSmallCastleKing;
        _blacks ^= _blackSmallCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoWhiteBigCastle()
    {
        _pieces[Squares.E1] = Pieces.WhiteKing;
        _pieces[Squares.A1] = Pieces.WhiteRook;

        _hash = _hash ^ _hashTable[Squares.D1][Pieces.WhiteRook] ^ _hashTable[Squares.A1][Pieces.WhiteRook];
        _hash = _hash ^ _hashTable[Squares.C1][Pieces.WhiteKing] ^ _hashTable[Squares.E1][Pieces.WhiteKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.WhiteKing) ^= _whiteBigCastleKing;
        Unsafe.Add(ref boardBase, Pieces.WhiteRook) ^= _whiteBigCastleRook;

        _whites ^= _whiteBigCastleKing;
        _whites ^= _whiteBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoBlackBigCastle()
    {
        _pieces[Squares.E8] = Pieces.BlackKing;
        _pieces[Squares.A8] = Pieces.BlackRook;

        _hash = _hash ^ _hashTable[Squares.D8][Pieces.BlackRook] ^ _hashTable[Squares.A8][Pieces.BlackRook];
        _hash = _hash ^ _hashTable[Squares.C8][Pieces.BlackKing] ^ _hashTable[Squares.E8][Pieces.BlackKing];

        ref var boardBase = ref _boards[0];
        Unsafe.Add(ref boardBase, Pieces.BlackKing) ^= _blackBigCastleKing;
        Unsafe.Add(ref boardBase, Pieces.BlackRook) ^= _blackBigCastleRook;

        _blacks ^= _blackBigCastleKing;
        _blacks ^= _blackBigCastleRook;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackSmallCastle() => _moveHistory.CanDoBlackSmallCastle() && _empty.IsSet(_blackSmallCastleCondition) && Unsafe.Add(ref _boards[0], Pieces.BlackRook).IsSet(Squares.H8) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteSmallCastle() => _moveHistory.CanDoWhiteSmallCastle() && _empty.IsSet(_whiteSmallCastleCondition) && Unsafe.Add(ref _boards[0], Pieces.WhiteRook).IsSet(Squares.H1) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoBlackBigCastle() => _moveHistory.CanDoBlackBigCastle() && _empty.IsSet(_blackBigCastleCondition) && Unsafe.Add(ref _boards[0], Pieces.BlackRook).IsSet(Squares.A8) && !_moveHistory.IsLastMoveWasCheck();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanDoWhiteBigCastle() => _moveHistory.CanDoWhiteBigCastle() && _empty.IsSet(_whiteBigCastleCondition) && Unsafe.Add(ref _boards[0], Pieces.WhiteRook).IsSet(Squares.A1) && !_moveHistory.IsLastMoveWasCheck();


    private void SetCastles()
    {
        _whiteSmallCastleCondition = new BitBoard();
        _whiteSmallCastleCondition = _whiteSmallCastleCondition.Set(5, 6);

        _whiteBigCastleCondition = new BitBoard();
        _whiteBigCastleCondition = _whiteBigCastleCondition.Set(1, 2, 3);

        _blackSmallCastleCondition = new BitBoard();
        _blackSmallCastleCondition = _blackSmallCastleCondition.Set(61, 62);

        _blackBigCastleCondition = new BitBoard();
        _blackBigCastleCondition = _blackBigCastleCondition.Set(57, 58, 59);

        _whiteBigCastleKing = new BitBoard();
        _whiteBigCastleKing = _whiteBigCastleKing.Or(4, 2);

        _whiteBigCastleRook = new BitBoard();
        _whiteBigCastleRook = _whiteBigCastleRook.Or(0, 3);

        _whiteSmallCastleKing = new BitBoard();
        _whiteSmallCastleKing = _whiteSmallCastleKing.Or(4, 6);

        _whiteSmallCastleRook = new BitBoard();
        _whiteSmallCastleRook = _whiteSmallCastleRook.Or(5, 7);

        _blackBigCastleKing = new BitBoard();
        _blackBigCastleKing = _blackBigCastleKing.Or(58, 60);

        _blackBigCastleRook = new BitBoard();
        _blackBigCastleRook = _blackBigCastleRook.Or(56, 59);

        _blackSmallCastleKing = new BitBoard();
        _blackSmallCastleKing = _blackSmallCastleKing.Or(60, 62);

        _blackSmallCastleRook = new BitBoard();
        _blackSmallCastleRook = _blackSmallCastleRook.Or(61, 63);
    }
}
