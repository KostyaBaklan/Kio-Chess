using Analysis.Core.Interfaces;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Pgn;

namespace Analysis.Core.Services;

/// <summary>
/// Service for parsing PGN (Portable Game Notation) strings into engine moves.
/// Refactored to use Engine.Pgn for fast, accurate parsing (replaces slow Regex approach).
/// Creates a fresh Position instance for each parse operation to avoid conflicts.
/// </summary>
public class PgnParserService : IPgnParserService
{
    private readonly IMoveFormatter _moveFormatter;

    public PgnParserService(IMoveFormatter moveFormatter)
    {
        _moveFormatter = moveFormatter;
    }

    /// <summary>
    /// Parses PGN string into engine moves using optimized Engine.Pgn parser.
    /// Much faster than previous Regex-based approach.
    /// </summary>
    public List<MoveBase> ParseMoves(string pgn)
    {
        var engineMoves = new List<MoveBase>();

        if (string.IsNullOrWhiteSpace(pgn))
            return engineMoves;

        var previousBoard = MoveBase.Board;
        var position = new Position();

        try
        {
            position.Clear();

            var reader = new PgnReader();
            var database = reader.ReadFromString(pgn);
            var game = database.Games.FirstOrDefault();

            if (game == null || game.Moves.Count == 0)
                return engineMoves;

            bool isFirstMove = true;
            bool isWhite = true;

            foreach (var pgnMove in game.Moves)
            {
                var legalMoves = isFirstMove
                    ? position.GetFirstMoves().ToList()
                    : position.GetAllMoves();

                var engineMove = FindEngineMove(pgnMove, legalMoves, isWhite);

                if (engineMove != null)
                {
                    engineMoves.Add(engineMove);

                    if (isFirstMove)
                    {
                        position.MakeFirst(engineMove);
                        isFirstMove = false;
                    }
                    else
                    {
                        position.Make(engineMove);
                    }

                    isWhite = !isWhite;
                }
                else
                {
                    break;
                }
            }
        }
        catch
        {
            // Return what we parsed so far
        }
        finally
        {
            position.Clear();
            MoveBase.Board = previousBoard;
        }

        return engineMoves;
    }

    /// <summary>
    /// Extracts PGN headers/tags using Engine.Pgn parser (replaces Regex).
    /// </summary>
    public Dictionary<string, string> ParseHeaders(string pgn)
    {
        if (string.IsNullOrWhiteSpace(pgn))
            return new Dictionary<string, string>();

        try
        {
            var reader = new PgnReader();
            var database = reader.ReadFromString(pgn);
            var game = database.Games.FirstOrDefault();

            return game?.Tags ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Finds the engine move that matches a PGN move.
    /// Uses multiple matching strategies for robustness.
    /// </summary>
    private MoveBase FindEngineMove(Engine.Pgn.Models.PgnMove pgnMove, List<MoveBase> legalMoves, bool isWhite)
    {
        // Strategy 1: Try UCI format if move looks like UCI (e.g., "e2e4")
        if (!string.IsNullOrEmpty(pgnMove.Notation) && pgnMove.Notation.Length >= 4 && pgnMove.Notation.Length <= 5)
        {
            var move = UciMoveConverter.FromUci(pgnMove.Notation, legalMoves);
            if (move != null)
                return move;
        }

        // Strategy 2: Try move formatter matching (most reliable)
        if (_moveFormatter != null)
        {
            var cleanNotation = pgnMove.Notation?.Replace("+", "").Replace("#", "").Trim();

            foreach (var move in legalMoves)
            {
                var formatted = _moveFormatter.Format(move);
                var cleanFormatted = formatted?.Replace("+", "").Replace("#", "").Trim();

                if (string.Equals(cleanFormatted, cleanNotation, StringComparison.OrdinalIgnoreCase))
                    return move;
            }
        }

        // Strategy 3: Use PgnMove properties to find the move
        return FindMoveByPgnProperties(pgnMove, legalMoves, isWhite);
    }

    /// <summary>
    /// Finds engine move using PGN move properties (piece, squares, castling).
    /// Filters by piece type first to avoid ambiguity between different piece types.
    /// </summary>
    private MoveBase FindMoveByPgnProperties(Engine.Pgn.Models.PgnMove pgnMove, List<MoveBase> legalMoves, bool isWhite)
    {
        // Handle castling
        if (pgnMove.Castling != Engine.Pgn.Models.CastlingType.None)
        {
            foreach (var move in legalMoves)
            {
                if (move.IsCastle)
                {
                    if (pgnMove.Castling == Engine.Pgn.Models.CastlingType.KingSide && move.To > move.From)
                        return move;
                    if (pgnMove.Castling == Engine.Pgn.Models.CastlingType.QueenSide && move.To < move.From)
                        return move;
                }
            }
            return null;
        }

        // Handle regular moves - match by target square
        if (string.IsNullOrEmpty(pgnMove.TargetSquare))
            return null;

        var targetSquare = SquareNameToIndex(pgnMove.TargetSquare);
        if (targetSquare == null)
            return null;

        // Filter by piece type FIRST to avoid matching wrong piece type
        var pieceTypeCandidates = FilterByPieceType(legalMoves, pgnMove.Piece, isWhite);
        var candidateMoves = pieceTypeCandidates.Where(m => m.To == targetSquare.Value).ToList();

        if (candidateMoves.Count == 0)
            return null;

        if (candidateMoves.Count == 1)
            return candidateMoves[0];

        // Disambiguation needed
        if (!string.IsNullOrEmpty(pgnMove.OriginSquare))
        {
            var originSquare = SquareNameToIndex(pgnMove.OriginSquare);
            if (originSquare != null)
            {
                return candidateMoves.FirstOrDefault(m => m.From == originSquare.Value);
            }
        }

        if (pgnMove.OriginFile.HasValue)
        {
            var file = pgnMove.OriginFile.Value - 'a';
            return candidateMoves.FirstOrDefault(m => (m.From % 8) == file);
        }

        if (pgnMove.OriginRank.HasValue)
        {
            var rank = pgnMove.OriginRank.Value - '1';
            return candidateMoves.FirstOrDefault(m => (m.From / 8) == rank);
        }

        if (pgnMove.PromotionPiece.HasValue)
        {
            return candidateMoves.OfType<PromotionMove>().FirstOrDefault();
        }

        // If still multiple candidates, return first
        return candidateMoves.FirstOrDefault();
    }

    /// <summary>
    /// Converts square name like "e4" to board index (0-63).
    /// </summary>
    private byte? SquareNameToIndex(string squareName)
    {
        if (string.IsNullOrEmpty(squareName) || squareName.Length != 2)
            return null;

        var file = char.ToLower(squareName[0]) - 'a';
        var rank = squareName[1] - '1';

        if (file < 0 || file > 7 || rank < 0 || rank > 7)
            return null;

        return (byte)(rank * 8 + file);
    }

    /// <summary>
    /// Filters moves by PGN piece type.
    /// Critical for disambiguating when multiple piece types can reach the same square.
    /// </summary>
    private List<MoveBase> FilterByPieceType(List<MoveBase> moves, Engine.Pgn.Models.PieceType pieceType, bool isWhite)
    {
        // Map PGN piece type to engine piece values
        var targetPieces = new List<byte>();

        switch (pieceType)
        {
            case Engine.Pgn.Models.PieceType.Pawn:
                targetPieces.Add(isWhite ? Engine.Models.Enums.Pieces.WhitePawn : Engine.Models.Enums.Pieces.BlackPawn);
                break;
            case Engine.Pgn.Models.PieceType.Knight:
                targetPieces.Add(isWhite ? Engine.Models.Enums.Pieces.WhiteKnight : Engine.Models.Enums.Pieces.BlackKnight);
                break;
            case Engine.Pgn.Models.PieceType.Bishop:
                targetPieces.Add(isWhite ? Engine.Models.Enums.Pieces.WhiteBishop : Engine.Models.Enums.Pieces.BlackBishop);
                break;
            case Engine.Pgn.Models.PieceType.Rook:
                targetPieces.Add(isWhite ? Engine.Models.Enums.Pieces.WhiteRook : Engine.Models.Enums.Pieces.BlackRook);
                break;
            case Engine.Pgn.Models.PieceType.Queen:
                targetPieces.Add(isWhite ? Engine.Models.Enums.Pieces.WhiteQueen : Engine.Models.Enums.Pieces.BlackQueen);
                break;
            case Engine.Pgn.Models.PieceType.King:
                targetPieces.Add(isWhite ? Engine.Models.Enums.Pieces.WhiteKing : Engine.Models.Enums.Pieces.BlackKing);
                break;
        }

        return moves.Where(m => targetPieces.Contains(m.Piece)).ToList();
    }
}


