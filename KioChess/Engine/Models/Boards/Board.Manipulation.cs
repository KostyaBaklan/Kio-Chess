using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveWhite(byte piece, byte square)
    {
        Hash = Hash ^ _hashTable[square * 12 + piece];

        var bit = ~square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) &= bit;
        Whites &= bit;

        Occupied = Whites | Blacks;
        Empty = ~Occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWhite(byte piece, byte square)
    {
        Hash = Hash ^ _hashTable[square * 12 + piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) |= bitBoard;
        Whites |= bitBoard;

        Occupied = Whites | Blacks;
        Empty = ~Occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveWhite(byte piece, byte from, byte to)
    {
        Hash = Hash ^ _hashTable[from * 12 + piece] ^ _hashTable[to * 12 + piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) ^= bitBoard;
        Whites ^= bitBoard;

        Occupied = Whites | Blacks;
        Empty = ~Occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBlack(byte piece, byte square)
    {
        Hash = Hash ^ _hashTable[square * 12 + piece];

        var bit = ~square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) &= bit;
        Blacks &= bit;

        Occupied = Whites | Blacks;
        Empty = ~Occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBlack(byte piece, byte square)
    {
        Hash = Hash ^ _hashTable[square * 12 + piece];
        _pieces[square] = piece;

        BitBoard bitBoard = square.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) |= bitBoard;
        Blacks |= bitBoard;

        Occupied = Whites | Blacks;
        Empty = ~Occupied;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveBlack(byte piece, byte from, byte to)
    {
        Hash = Hash ^ _hashTable[from * 12 + piece] ^ _hashTable[to * 12 + piece];
        _pieces[to] = piece;

        BitBoard bitBoard = from.AsBitBoard() | to.AsBitBoard();

        Unsafe.Add(ref _boards[0], piece) ^= bitBoard;
        Blacks ^= bitBoard;

        Occupied = Whites | Blacks;
        Empty = ~Occupied;
    }
}