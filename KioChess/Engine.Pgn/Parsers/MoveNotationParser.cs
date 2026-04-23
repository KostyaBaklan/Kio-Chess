using Engine.Pgn.Models;

namespace Engine.Pgn.Parsers;

public static class MoveNotationParser
{
    public static PgnMove ParseMove(ReadOnlySpan<char> notation)
    {
        var move = new PgnMove { Notation = notation.ToString() };
        var span = notation;

        if (span.IsEmpty)
        {
            return move;
        }

        if (IsCastling(span, out var castlingType))
        {
            move.Piece = PieceType.King;
            move.Castling = castlingType;
            move.TargetSquare = castlingType == CastlingType.KingSide ? "g" : "c";
            return move;
        }

        int index = 0;

        if (char.IsUpper(span[0]))
        {
            move.Piece = ParsePieceType(span[0]);
            index = 1;
        }
        else
        {
            move.Piece = PieceType.Pawn;
        }

        ParseOriginHints(span, ref index, move);

        if (index < span.Length && (span[index] == 'x' || span[index] == ':'))
        {
            move.IsCapture = true;
            index++;
        }

        if (index + 1 < span.Length && char.IsLetter(span[index]) && char.IsDigit(span[index + 1]))
        {
            move.TargetSquare = span.Slice(index, 2).ToString().ToLower();
            index += 2;
        }

        if (index < span.Length && span[index] == '=')
        {
            index++;
            if (index < span.Length)
            {
                move.PromotionPiece = ParsePieceType(span[index]);
                index++;
            }
        }

        ParseCheckAnnotations(span, index, move);

        return move;
    }

    private static bool IsCastling(ReadOnlySpan<char> notation, out CastlingType castlingType)
    {
        castlingType = CastlingType.None;

        var cleaned = RemoveAnnotations(notation);

        if (cleaned.SequenceEqual("O-O".AsSpan()) || cleaned.SequenceEqual("0-0".AsSpan()))
        {
            castlingType = CastlingType.KingSide;
            return true;
        }

        if (cleaned.SequenceEqual("O-O-O".AsSpan()) || cleaned.SequenceEqual("0-0-0".AsSpan()))
        {
            castlingType = CastlingType.QueenSide;
            return true;
        }

        return false;
    }

    private static ReadOnlySpan<char> RemoveAnnotations(ReadOnlySpan<char> notation)
    {
        int end = notation.Length;
        while (end > 0 && (notation[end - 1] == '+' || notation[end - 1] == '#' || notation[end - 1] == '!' || notation[end - 1] == '?'))
        {
            end--;
        }
        return notation.Slice(0, end);
    }

    private static void ParseOriginHints(ReadOnlySpan<char> span, ref int index, PgnMove move)
    {
        if (move.Piece == PieceType.Pawn)
        {
            if (index < span.Length && char.IsLetter(span[index]))
            {
                var file = span[index];
                if (index + 1 < span.Length && (span[index + 1] == 'x' || span[index + 1] == ':'))
                {
                    move.OriginFile = file;
                    index++; // Move past the origin file letter
                }
            }
        }
        else
        {
            // For pieces (not pawns), we need to parse disambiguation hints
            // Examples: Nbd7, R1a3, Qh4e1
            // But "Nf3" should NOT consume 'f' as origin - it's the target!
            
            while (index < span.Length)
            {
                char c = span[index];

                if (c == 'x' || c == ':')
                {
                    break;
                }

                if (char.IsLetter(c) && c >= 'a' && c <= 'h')
                {
                    // Check if this is a complete square (e.g., "e2" in "Ne2e4")
                    if (index + 1 < span.Length && char.IsDigit(span[index + 1]))
                    {
                        // This looks like a square. Is it origin or target?
                        // It's origin only if there's more content after (another square or 'x')
                        bool hasMoreAfter = index + 2 < span.Length && 
                                          (span[index + 2] == 'x' || span[index + 2] == ':' ||
                                           (char.IsLetter(span[index + 2]) && span[index + 2] >= 'a' && span[index + 2] <= 'h'));
                        
                        if (hasMoreAfter)
                        {
                            // It's the origin square
                            move.OriginSquare = span.Slice(index, 2).ToString().ToLower();
                            index += 2;
                        }
                        else
                        {
                            // It's the target square, stop parsing hints
                            break;
                        }
                    }
                    else if (move.OriginFile == null)
                    {
                        // Single letter - could be file disambiguation (e.g., "Nbd7")
                        // But only if there's something after it (not end of notation)
                        // Check if there's a target square coming
                        bool hasTargetAfter = false;
                        int lookAhead = index + 1;
                        
                        // Skip 'x' if present
                        if (lookAhead < span.Length && (span[lookAhead] == 'x' || span[lookAhead] == ':'))
                        {
                            lookAhead++;
                        }
                        
                        // Check if there's a square after
                        if (lookAhead + 1 < span.Length && 
                            char.IsLetter(span[lookAhead]) && span[lookAhead] >= 'a' && span[lookAhead] <= 'h' &&
                            char.IsDigit(span[lookAhead + 1]))
                        {
                            hasTargetAfter = true;
                        }
                        
                        if (hasTargetAfter)
                        {
                            move.OriginFile = c;
                            index++;
                        }
                        else
                        {
                            // No target after, so this letter is part of the target
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else if (char.IsDigit(c) && move.OriginRank == null)
                {
                    move.OriginRank = c;
                    index++;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private static void ParseCheckAnnotations(ReadOnlySpan<char> span, int index, PgnMove move)
    {
        while (index < span.Length)
        {
            char c = span[index];
            if (c == '+')
            {
                move.IsCheck = true;
            }
            else if (c == '#')
            {
                move.IsCheckmate = true;
                move.IsCheck = true;
            }
            index++;
        }
    }

    private static PieceType ParsePieceType(char pieceChar)
    {
        return char.ToUpper(pieceChar) switch
        {
            'N' => PieceType.Knight,
            'B' => PieceType.Bishop,
            'R' => PieceType.Rook,
            'Q' => PieceType.Queen,
            'K' => PieceType.King,
            _ => PieceType.Pawn
        };
    }
}
