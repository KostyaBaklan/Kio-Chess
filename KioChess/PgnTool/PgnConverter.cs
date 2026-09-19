using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Pgn.Models;

namespace PgnTool;

internal static class PgnConverter
{
    private static readonly Dictionary<string, byte> _squares = new();
    private static readonly Dictionary<string, byte> _pieces = new();

    static PgnConverter()
    {
        for (byte i = 0; i < 64; i++)
        {
            var k = i.AsString().ToLower();
            _squares[k] = i;
        }

        for (byte i = 0; i < 12; i++)
        {
            var p = i.AsEnumString();
            _pieces[p] = i;
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
            var squareString = move.TargetSquare.ToLower();
            var square = _squares[squareString];

            var piecePrefix = isWhite ? "White" : "Black";
            var pieceString = $"{piecePrefix}{move.Piece}";
            var piece = _pieces[pieceString];

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
        }
        else
        {
            MoveBase selectedMove = null;

            if (!string.IsNullOrEmpty(move.OriginSquare))
            {
                string squareString = move.OriginSquare.ToLower();
                var square = _squares[squareString];
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
                var piecePrefix = isWhite ? "White" : "Black";
                var pieceString = $"{piecePrefix}{move.PromotionPiece.Value}";
                var piece = _pieces[pieceString];
                selectedMove = moves.OfType<PromotionMove>().FirstOrDefault(m => m.PromotionPiece == piece);
            }

            if (selectedMove == null)
            {
                throw new ArgumentException($"Could not disambiguate move {move.Notation}");
            }

            position.Make(selectedMove);
        }
    }

    public static GameResult ConvertResult(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
            return GameResult.None;

        result = result.Replace(" ", "").ToLower();

        return result switch
        {
            "1-0" or "white" => GameResult.White,
            "0-1" or "black" => GameResult.Black,
            "1/2-1/2" or "0.5-0.5" or "draw" => GameResult.Draw,
            _ => GameResult.None
        };
    }
}
