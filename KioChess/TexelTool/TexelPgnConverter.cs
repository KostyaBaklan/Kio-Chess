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

        IEnumerable<MoveBase> candidates = moves;
        if (move.PromotionPiece.HasValue)
        {
            var promo = _pieces[$"{(isWhite ? "White" : "Black")}{move.PromotionPiece.Value}"];
            candidates = candidates.Where(m => (m is PromotionMove pm && pm.PromotionPiece == promo)
                                            || (m is PromotionAttack pa && pa.PromotionPiece == promo));
        }
        if (!string.IsNullOrEmpty(move.OriginSquare))
        {
            var from = _squares[move.OriginSquare.ToLower()];
            candidates = candidates.Where(m => m.From == from);
        }
        else
        {
            if (move.OriginFile.HasValue)
                candidates = candidates.Where(m => m.From.AsString().StartsWith(move.OriginFile.Value.ToString(), StringComparison.OrdinalIgnoreCase));
            if (move.OriginRank.HasValue)
                candidates = candidates.Where(m => m.From.AsString().EndsWith(move.OriginRank.Value.ToString()));
        }
        var list = candidates.ToList();
        if (list.Count != 1)
            throw new ArgumentException($"Could not disambiguate move {move.Notation} ({list.Count} candidates)");
        if (position.GetHistory().Any())
            position.Make(list[0]);
        else
            position.MakeFirst(list[0]);
    }
}
