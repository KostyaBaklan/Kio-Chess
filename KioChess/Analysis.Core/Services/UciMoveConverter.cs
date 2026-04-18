using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Analysis.Core.Services;

/// <summary>
/// Converts between UCI move strings (e.g. <c>"e2e4"</c>, <c>"e7e8q"</c>)
/// and engine <see cref="MoveBase"/> objects.
/// </summary>
public static class UciMoveConverter
{
    /// <summary>
    /// Returns the UCI string for a <see cref="MoveBase"/>.
    /// Promotion pieces are appended as q/r/b/n.
    /// </summary>
    public static string ToUci(MoveBase move)
    {
        var from = move.From.AsString().ToLower();
        var to   = move.To.AsString().ToLower();

        if (move.IsPromotion)
        {
            char promo = move.Piece switch
            {
                Engine.Models.Enums.Pieces.WhiteQueen  or Engine.Models.Enums.Pieces.BlackQueen  => 'q',
                Engine.Models.Enums.Pieces.WhiteRook   or Engine.Models.Enums.Pieces.BlackRook   => 'r',
                Engine.Models.Enums.Pieces.WhiteBishop or Engine.Models.Enums.Pieces.BlackBishop => 'b',
                Engine.Models.Enums.Pieces.WhiteKnight or Engine.Models.Enums.Pieces.BlackKnight => 'n',
                _ => 'q'
            };
            return $"{from}{to}{promo}";
        }

        return $"{from}{to}";
    }

    /// <summary>
    /// Builds the space-separated UCI move sequence string for all moves in the list.
    /// </summary>
    public static string BuildMoveSequence(IEnumerable<MoveBase> moves)
        => string.Join(" ", moves.Select(ToUci));

    /// <summary>
    /// Finds the <see cref="MoveBase"/> in <paramref name="legalMoves"/> that matches
    /// the UCI string <paramref name="uci"/>. Returns <c>null</c> if not found.
    /// </summary>
    public static MoveBase FromUci(string uci, IEnumerable<MoveBase> legalMoves)
    {
        if (string.IsNullOrWhiteSpace(uci) || uci.Length < 4)
            return null;

        // Parse from/to squares
        var fromStr = uci[..2].ToUpper();
        var toStr   = uci[2..4].ToUpper();
        byte from   = fromStr.GetIndex();
        byte to     = toStr.GetIndex();

        char? promoChar = uci.Length > 4 ? char.ToLower(uci[4]) : null;

        foreach (var move in legalMoves)
        {
            if (move.From != from || move.To != to) continue;

            // Match promotion piece if required
            if (promoChar.HasValue && move.IsPromotion)
            {
                char expected = move.Piece switch
                {
                    Engine.Models.Enums.Pieces.WhiteQueen  or Engine.Models.Enums.Pieces.BlackQueen  => 'q',
                    Engine.Models.Enums.Pieces.WhiteRook   or Engine.Models.Enums.Pieces.BlackRook   => 'r',
                    Engine.Models.Enums.Pieces.WhiteBishop or Engine.Models.Enums.Pieces.BlackBishop => 'b',
                    Engine.Models.Enums.Pieces.WhiteKnight or Engine.Models.Enums.Pieces.BlackKnight => 'n',
                    _ => 'q'
                };
                if (expected == promoChar.Value) return move;
                continue;
            }

            // For non-promotions: first legal move with matching from/to wins
            if (!move.IsPromotion) return move;
        }

        return null;
    }
}
