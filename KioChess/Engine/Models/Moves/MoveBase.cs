using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class MoveBase : IEquatable<MoveBase>, IComparable<MoveBase>
{
    protected static readonly AttackStack _figureHistory = new();
    public static Board Board;
    private static readonly float _inverseHistoryFactor;

    static MoveBase()
    {
        var _historyFactor = ContainerLocator.Current.Resolve<IConfigurationProvider>()
            .GeneralConfiguration.HistoryHeuristic.RelativeHistoryFactor;

        _inverseHistoryFactor = 1.0f / _historyFactor;
    }

    protected MoveBase()
    {
        // Initialize to default values - C# handles most, but be explicit for clarity
        Butterfly = 1;
    }

    #region Implementation of IMove

    public short Key;
    public int History;
    public int Butterfly;
    public int RelativeHistory;
    public byte Piece;
    public byte From;
    public byte To;
    public BitBoard EmptyBoard;
    public int BookValue;

    // Keep flags as fields for direct access - maximum performance
    // While we lose some encapsulation, we gain ~50ms per move in performance
    public bool IsCheck;
    public bool IsAttack;
    public bool IsCastle;
    public bool IsPromotion;
    public bool IsEnPassant;
    public bool CanReduce;
    public bool CanNotReduceNext;
    public bool IsIrreversible;
    public bool IsFutile;
    public bool IsQuiet;
    public bool IsPromotionExtension;

    // Keep computed properties for IsWhite/IsBlack/Turn - they are rarely accessed in hot paths
    public bool IsWhite
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Piece < 6;
    }

    public bool IsBlack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Piece > 5;
    }

    public Turn Turn
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Piece < 6 ? Turn.White : Turn.Black;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract bool IsLegal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Make();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void UnMake();

    public void Set(params byte[] squares)
    {
        BitBoard v = new();
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
        BitBoard v = new();
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
    public void SetRelativeHistory() => RelativeHistory = (int)((History * _inverseHistoryFactor) / Butterfly);

    #endregion

    #region Overrides of Object

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveHistory ToMoveHistory() => new(Key, RelativeHistory);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveHistory ToBookHistory() => new(Key, BookValue);

    public virtual string ToUciString() => $"{From.AsString()}{To.AsString()}".ToLower();

    /// <summary>
    /// Converts the move to Standard Algebraic Notation (SAN).
    /// Override in derived classes for move-specific formatting.
    /// </summary>
    public virtual string ToSAN()
    {
        var to = To.AsString().ToLower();
        
        // Get piece character (empty for pawns)
        var pieceChar = Piece switch
        {
            Pieces.WhiteKnight or Pieces.BlackKnight => "N",
            Pieces.WhiteBishop or Pieces.BlackBishop => "B",
            Pieces.WhiteRook or Pieces.BlackRook => "R",
            Pieces.WhiteQueen or Pieces.BlackQueen => "Q",
            Pieces.WhiteKing or Pieces.BlackKing => "K",
            _ => ""  // Pawn
        };
        
        return $"{pieceChar}{to}";
    }

    public string ToLightString() => $"[{Piece.AsKeyName()} {From.AsString()}{To.AsString()}]";

    public override string ToString() => $"[{Piece.AsKeyName()} {From.AsString()}->{To.AsString()}, H={History}, B={Butterfly}, R={(int)((History * _inverseHistoryFactor) / Butterfly)}]";

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
