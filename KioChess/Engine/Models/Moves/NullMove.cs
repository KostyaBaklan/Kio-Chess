using System.Runtime.CompilerServices;
namespace Engine.Models.Moves;

/// <summary>Placeholder pushed to MoveHistory for a null move. Make/UnMake are no-ops.</summary>
public sealed class NullMove : MoveBase
{
    public NullMove(short key, byte piece) { Key = key; Piece = piece; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool IsLegal() => false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override void Make() { }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override void UnMake() { }
    public override string ToUciString() => "0000";
}
