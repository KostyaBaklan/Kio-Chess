using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Pgn.Models;

namespace NnueTool;

internal static class PgnMoveResolver
{
    private static readonly Dictionary<string, byte> _squares = new();

    static PgnMoveResolver()
    {
        for (byte i = 0; i < 64; i++)
        {
            _squares[i.AsString().ToLower()] = i;
        }
    }

    public static MoveBase Resolve(Position position, PgnMove move, bool isWhite)
    {
        List<MoveBase> moves;

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
        else
        {
            if (string.IsNullOrEmpty(move.TargetSquare)
                || !_squares.TryGetValue(move.TargetSquare.ToLower(), out var square))
                return null;

            byte piece = (byte)((isWhite ? 0 : 6) + (int)move.Piece);
            moves = position.GetMoves(piece, square);
        }

        if (moves.Count == 0)
            return null;

        if (moves.Count == 1)
            return moves[0];

        MoveBase selected = null;
        int matches = 0;

        foreach (var candidate in moves)
        {
            if (!Matches(candidate, move, isWhite))
                continue;

            selected = candidate;
            matches++;
        }

        return matches == 1 ? selected : null;
    }

    private static bool Matches(MoveBase candidate, PgnMove move, bool isWhite)
    {
        if (!string.IsNullOrEmpty(move.OriginSquare)
            && (!_squares.TryGetValue(move.OriginSquare.ToLower(), out var from) || candidate.From != from))
            return false;

        if (move.OriginFile.HasValue
            && char.ToLower(candidate.From.AsString()[0]) != char.ToLower(move.OriginFile.Value))
            return false;

        if (move.OriginRank.HasValue
            && candidate.From.AsString()[1] != move.OriginRank.Value)
            return false;

        if (move.PromotionPiece.HasValue)
        {
            byte promotion = (byte)((isWhite ? 0 : 6) + (int)move.PromotionPiece.Value);
            if (candidate is not PromotionMove promotionMove || promotionMove.PromotionPiece != promotion)
                return false;
        }

        return true;
    }
}
