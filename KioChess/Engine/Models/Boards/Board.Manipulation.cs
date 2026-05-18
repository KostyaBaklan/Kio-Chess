using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveWhite(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];

        var bit = ~square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) &= bit;
        _whites &= bit;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWhite(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) |= bitBoard;
        _whites |= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveWhite(byte piece, byte from, byte to)
    {
        _hash = _hash ^ _hashTable[from][piece] ^ _hashTable[to][piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) ^= bitBoard;
        _whites ^= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBlack(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];

        var bit = ~square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) &= bit;
        _blacks &= bit;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBlack(byte piece, byte square)
    {
        _hash = _hash ^ _hashTable[square][piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) |= bitBoard;
        _blacks |= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveBlack(byte piece, byte from, byte to)
    {
        _hash = _hash ^ _hashTable[from][piece] ^ _hashTable[to][piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) ^= bitBoard;
        _blacks ^= bitBoard;

        _occupied = _whites | _blacks;
        _empty = ~_occupied;
    }
}