using Engine.DataStructures.Moves;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class AttackBase : MoveBase, IComparable<AttackBase>
{
    public byte Captured;
    public int See;
    public static int[] CapturedValue;

    protected AttackBase()
    {
        IsAttack = true;
    }

    #region Implementation of IComparable<in AttackBase>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(AttackBase other) => other.See.CompareTo(See);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(AttackBase move) => See > move.See;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLess(AttackBase move) => Piece < move.Piece;

    #endregion


    public override string ToString() => $"[{Piece.AsKeyName()} {From.AsString()} x {To.AsString()}, S={See}, B={BookValue}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetCapturedValue() => See = CapturedValue[Board.GetPiece(To)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int GetCapturedValue() => CapturedValue[Captured];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetCapturedPiece() => Captured = Board.GetPiece(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetCapturedValue(byte piece) => CapturedValue[piece];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveHistory ToCaptureHistory() => new(Key, See);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetMvvLvaScore() => CapturedValue[Captured] - CapturedValue[Piece];
    
    /// <summary>
    /// Converts attack to SAN notation with capture symbol (x).
    /// </summary>
    public override string ToSAN()
    {
        var from = From.AsString().ToLower();
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
        
        // For pawn captures, include origin file
        if (string.IsNullOrEmpty(pieceChar))
        {
            return $"{from[0]}x{to}";
        }
        
        // For piece captures
        return $"{pieceChar}x{to}";
    }
}