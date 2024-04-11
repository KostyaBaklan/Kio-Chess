using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves;

public abstract class MoveBase : IEquatable<MoveBase>, IComparable<MoveBase>
{
    protected static readonly ArrayStack<byte> _figureHistory = new ArrayStack<byte>();
    public static Board Board;

    protected MoveBase()
    {
        IsCheck = false;
        EmptyBoard = new BitBoard(0ul);
        IsAttack = false;
        IsPromotion = false;
        IsCastle = false;
        IsEnPassant = false;
        IsPromotionToQueen = false;
        Butterfly = 1;
    }

    #region Implementation of IMove

    public short Key;
    public bool IsCheck;
    public int History;
    public int Butterfly;
    public int RelativeHistory;
    public byte Piece;
    public byte From;
    public byte To;
    public BitBoard EmptyBoard;
    public bool IsAttack;
    public bool IsCastle;
    public bool IsPromotion;
    public bool IsPassed;
    public bool IsEnPassant;
    public bool CanReduce;
    public bool IsIrreversible;
    public bool IsFutile;
    public bool IsPromotionToQueen;
    public bool IsWhite;
    public bool IsBlack;
    public bool IsPromotionExtension;
    public Turn Turn;
    public int BookValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract bool IsLegal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool IsLegalAttack() => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Make();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void UnMake();

    public void Set(params byte[] squares)
    {
        BitBoard v = new BitBoard();
        for (var index = 0; index < squares.Length; index++)
        {
            var s = squares[index];
            var board = s.AsBitBoard();
            v = v | board;
        }

        EmptyBoard = EmptyBoard |= v;
    }
    public void Set(params int[] squares)
    {
        BitBoard v = new BitBoard();
        for (var index = 0; index < squares.Length; index++)
        {
            byte s = (byte)squares[index];
            var board = s.AsBitBoard();
            v = v | board;
        }

        EmptyBoard = EmptyBoard |= v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBookGreater(MoveBase move) => BookValue > move.BookValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(MoveBase move) => RelativeHistory > move.RelativeHistory;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRelativeHistory() => RelativeHistory = History / Butterfly;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual bool IsQueenCaptured() => false;

    #endregion

    #region Overrides of Object

    public string ToLightString() => $"[{Piece.AsKeyName()} {From.AsString()}{To.AsString()}]";

    public override string ToString() => $"[{Piece.AsKeyName()} {From.AsString()}->{To.AsString()}, H={History}, B={Butterfly}, R={History / Butterfly}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => !ReferenceEquals(null, obj) && Equals((MoveBase)obj);

    #region Equality members

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(MoveBase other) => Key == other.Key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => Key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(MoveBase other) => other.RelativeHistory.CompareTo(RelativeHistory);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(MoveBase left, MoveBase right)
    {
        return Equals(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(MoveBase left, MoveBase right)
    {
        return !Equals(left, right);
    }

    #endregion

    #endregion
}
