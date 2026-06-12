namespace Engine.Pgn.Models;

public class PgnMove
{
    public string Notation { get; set; }
    public PieceType Piece { get; set; }
    public string TargetSquare { get; set; }
    public string OriginSquare { get; set; }
    public char? OriginFile { get; set; }
    public char? OriginRank { get; set; }
    public PieceType? PromotionPiece { get; set; }
    public bool IsCapture { get; set; }
    public bool IsCheck { get; set; }
    public bool IsCheckmate { get; set; }
    public CastlingType Castling { get; set; }

    public override string ToString() => Notation;
}
