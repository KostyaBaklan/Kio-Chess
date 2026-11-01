using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Pgn.Models;

namespace TexelTool;

internal static class TexelPgnConverter
{
    private static readonly Dictionary<string, byte> _squares = new();
    private static readonly Dictionary<string, byte> _pieces = new();

    static TexelPgnConverter()
    {
        for (byte i = 0; i < 64; i++)
        {
            _squares[i.AsString().ToLower()] = i;
        }

        for (byte i = 0; i < 12; i++)
        {
            _pieces[i.AsEnumString()] = i;
        }
    }

    public static void ProcessMove(Position position, PgnMove move, bool isWhite)
    {
        List<MoveBase> moves = null;

        if (move.Castling != CastlingType.None)
        {
            if (isWhite)
            {
                moves = move.Castling == CastlingType.KingSide
                    ? position.GetMoves(Pieces.WhiteKing, Squares.G1)
                    : position.GetMoves(Pieces.WhiteKing, Squares.C1);
            }
            else
            {
                moves = move.Castling == CastlingType.KingSide
                    ? position.GetMoves(Pieces.BlackKing, Squares.G8)
                    : position.GetMoves(Pieces.BlackKing, Squares.C8);
            }
        }
        else if (!string.IsNullOrEmpty(move.TargetSquare))
        {
            var square = _squares[move.TargetSquare.ToLower()];
            var piece = _pieces[$"{(isWhite ? "White" : "Black")}{move.Piece}"];

            moves = position.GetMoves(piece, square);
        }

        if (moves == null || moves.Count == 0)
        {
            throw new ArgumentException($"Move list is empty trying to find {move.Notation}");
        }

        if (moves.Count == 1)
        {
            if (position.GetHistory().Any())
                position.Make(moves[0]);
            else
                position.MakeFirst(moves[0]);
            return;
        }

        MoveBase selectedMove = null;

        if (!string.IsNullOrEmpty(move.OriginSquare))
        {
            var square = _squares[move.OriginSquare.ToLower()];
            selectedMove = moves.FirstOrDefault(m => m.From == square);
        }
        else if (move.OriginFile.HasValue)
        {
            selectedMove = moves.FirstOrDefault(m =>
                m.From.AsString().StartsWith(move.OriginFile.Value.ToString(), StringComparison.OrdinalIgnoreCase));
        }
        else if (move.OriginRank.HasValue)
        {
            selectedMove = moves.FirstOrDefault(m =>
                m.From.AsString().EndsWith(move.OriginRank.Value.ToString()));
        }
        else if (move.PromotionPiece.HasValue)
        {
            var piece = _pieces[$"{(isWhite ? "White" : "Black")}{move.PromotionPiece.Value}"];
            selectedMove = moves.OfType<PromotionMove>().FirstOrDefault(m => m.PromotionPiece == piece);
        }

        if (selectedMove == null)
        {
            throw new ArgumentException($"Could not disambiguate move {move.Notation}");
        }

        position.Make(selectedMove);
    }
}
