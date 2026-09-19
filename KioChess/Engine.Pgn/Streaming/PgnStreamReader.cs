using Engine.Pgn.Models;
using Engine.Pgn.Parsers;
using System.Text;

namespace Engine.Pgn.Streaming;

/// <summary>
/// High-performance streaming reader for large PGN files.
/// Lazily enumerates games without loading entire file into memory.
/// Optimized for files 200GB+ in size.
/// </summary>
public class PgnStreamReader : IDisposable
{
    private readonly StreamReader _reader;
    private readonly bool _ownsStream;
    private bool _disposed;

    public PgnStreamReader(string filePath)
    {
        _reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 65536);
        _ownsStream = true;
    }

    public PgnStreamReader(Stream stream, bool leaveOpen = false)
    {
        _reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 65536, leaveOpen: leaveOpen);
        _ownsStream = !leaveOpen;
    }

    /// <summary>
    /// Gets the current position in the stream for progress tracking
    /// </summary>
    public long Position => _reader.BaseStream.Position;

    /// <summary>
    /// Gets the total length of the stream
    /// </summary>
    public long Length => _reader.BaseStream.Length;

    /// <summary>
    /// Gets the progress percentage (0-100)
    /// </summary>
    public double ProgressPercent => Length > 0 ? (100.0 * Position / Length) : 0;

    /// <summary>
    /// Lazily enumerates all games in the PGN file.
    /// Games are parsed on-demand as you iterate.
    /// </summary>
    public IEnumerable<PgnGame> ReadGames()
    {
        StringBuilder gameText = new StringBuilder(4096);
        PgnGame currentGame = null;
        bool inMoveText = false;

        while (!_reader.EndOfStream)
        {
            var line = _reader.ReadLine();
            if (line == null) break;

            var trimmedLine = line.Trim();

            // Empty line - might separate sections
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                if (inMoveText && gameText.Length > 0)
                {
                    gameText.AppendLine();
                }
                continue;
            }

            // Tag line
            if (TagParser.IsTag(trimmedLine))
            {
                // If we were collecting move text, finish previous game
                if (currentGame != null && inMoveText)
                {
                    ParseAndAddMoveText(currentGame, gameText.ToString());
                    yield return currentGame;
                    currentGame = null;
                    gameText.Clear();
                    inMoveText = false;
                }

                // Start new game or add tag to current
                if (currentGame == null)
                {
                    currentGame = new PgnGame();
                }

                if (TagParser.TryParseTag(trimmedLine, out var tagName, out var tagValue))
                {
                    currentGame.Tags[tagName] = tagValue;
                    
                    if (tagName.Equals("Result", StringComparison.OrdinalIgnoreCase))
                    {
                        currentGame.Result = ParseResult(tagValue);
                        currentGame.ResultString = tagValue;
                    }
                }
            }
            else
            {
                // Move text line
                inMoveText = true;
                gameText.AppendLine(trimmedLine);
            }
        }

        // Yield last game if exists
        if (currentGame != null)
        {
            if (gameText.Length > 0)
            {
                ParseAndAddMoveText(currentGame, gameText.ToString());
            }
            yield return currentGame;
        }
    }

    /// <summary>
    /// Lazily enumerates games with lightweight metadata only (no move parsing).
    /// Much faster when you only need tags (Event, Players, ELO, etc.)
    /// </summary>
    public IEnumerable<PgnGameInfo> ReadGameInfo()
    {
        PgnGameInfo currentInfo = null;
        bool foundMoveText = false;

        while (!_reader.EndOfStream)
        {
            var line = _reader.ReadLine();
            if (line == null) break;

            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            // Tag line
            if (TagParser.IsTag(trimmedLine))
            {
                // If we found move text, yield previous game
                if (currentInfo != null && foundMoveText)
                {
                    yield return currentInfo;
                    currentInfo = null;
                    foundMoveText = false;
                }

                // Start new game or add tag to current
                if (currentInfo == null)
                {
                    currentInfo = new PgnGameInfo { FilePosition = Position };
                }

                if (TagParser.TryParseTag(trimmedLine, out var tagName, out var tagValue))
                {
                    currentInfo.Tags[tagName] = tagValue;
                }
            }
            else
            {
                // Found move text, mark for yielding
                foundMoveText = true;
            }
        }

        // Yield last game if exists
        if (currentInfo != null)
        {
            yield return currentInfo;
        }
    }

    /// <summary>
    /// Lazily enumerates game text as raw strings (tags + moves).
    /// Fastest option - no parsing at all, just text extraction.
    /// Perfect for passing directly to external tools.
    /// </summary>
    public IEnumerable<string> ReadGameText()
    {
        StringBuilder gameText = new StringBuilder(4096);
        bool hasContent = false;

        while (!_reader.EndOfStream)
        {
            var line = _reader.ReadLine();
            if (line == null) break;

            var trimmedLine = line.Trim();

            // Empty line might separate games
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                if (hasContent)
                {
                    gameText.AppendLine();
                }
                continue;
            }

            // Check if this is a new game starting (Event tag typically first)
            if (TagParser.IsTag(trimmedLine) && 
                trimmedLine.StartsWith("[Event ", StringComparison.OrdinalIgnoreCase))
            {
                // Yield previous game if we have one
                if (hasContent && gameText.Length > 0)
                {
                    yield return gameText.ToString();
                    gameText.Clear();
                }
                hasContent = true;
            }

            if (hasContent)
            {
                gameText.AppendLine(line);
            }
        }

        // Yield last game
        if (hasContent && gameText.Length > 0)
        {
            yield return gameText.ToString();
        }
    }

    private void ParseAndAddMoveText(PgnGame game, string moveText)
    {
        var cleaned = CleanMoveText(moveText);
        var tokens = TokenizeMoves(cleaned);

        foreach (var token in tokens)
        {
            if (IsGameResult(token))
            {
                game.Result = ParseResult(token);
                game.ResultString = token;
                continue;
            }

            if (IsMoveNumber(token))
            {
                continue;
            }

            var move = MoveNotationParser.ParseMove(token);
            game.Moves.Add(move);
        }
    }

    private string CleanMoveText(string moveText)
    {
        var sb = new StringBuilder(moveText.Length);
        bool inComment = false;
        bool inVariation = false;
        int variationDepth = 0;

        foreach (char c in moveText)
        {
            if (c == '{')
            {
                inComment = true;
                continue;
            }
            
            if (c == '}')
            {
                inComment = false;
                continue;
            }

            if (c == '(')
            {
                inVariation = true;
                variationDepth++;
                continue;
            }

            if (c == ')')
            {
                variationDepth--;
                if (variationDepth == 0)
                {
                    inVariation = false;
                }
                continue;
            }

            if (!inComment && !inVariation)
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private List<string> TokenizeMoves(string moveText)
    {
        var tokens = new List<string>();
        var sb = new StringBuilder();

        foreach (char c in moveText)
        {
            if (char.IsWhiteSpace(c))
            {
                if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0)
        {
            tokens.Add(sb.ToString());
        }

        return tokens;
    }

    private bool IsMoveNumber(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        if (!token.EndsWith('.') || token.Length <= 1)
            return false;

        for (int i = 0; i < token.Length - 1; i++)
        {
            if (!char.IsDigit(token[i]))
                return false;
        }

        return true;
    }

    private bool IsGameResult(string token)
    {
        return token == "1-0" || token == "0-1" || token == "1/2-1/2" || token == "*" ||
               token == "1 - 0" || token == "0 - 1" || token == "1/2 - 1/2";
    }

    private GameResult ParseResult(string result)
    {
        var normalized = result.Replace(" ", "").ToLower();
        
        return normalized switch
        {
            "1-0" => GameResult.White,
            "0-1" => GameResult.Black,
            "1/2-1/2" => GameResult.Draw,
            "0.5-0.5" => GameResult.Draw,
            "white" => GameResult.White,
            "black" => GameResult.Black,
            "draw" => GameResult.Draw,
            _ => GameResult.None
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_ownsStream)
            {
                _reader?.Dispose();
            }
            _disposed = true;
        }
    }
}
