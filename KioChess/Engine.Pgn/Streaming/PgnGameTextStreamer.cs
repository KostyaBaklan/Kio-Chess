namespace Engine.Pgn.Streaming;

/// <summary>
/// Ultra-fast game text streamer with inline ELO filtering.
/// Optimized for passing games directly to external tools without full parsing.
/// Perfect for 200GB+ files where you need game text + basic filtering.
/// </summary>
public class PgnGameTextStreamer : IDisposable
{
    private readonly StreamReader _reader;
    private readonly bool _ownsStream;
    private bool _disposed;

    public PgnGameTextStreamer(string filePath)
    {
        _reader = new StreamReader(filePath, System.Text.Encoding.UTF8, 
            detectEncodingFromByteOrderMarks: true, bufferSize: 65536);
        _ownsStream = true;
    }

    public PgnGameTextStreamer(Stream stream, bool leaveOpen = false)
    {
        _reader = new StreamReader(stream, System.Text.Encoding.UTF8, 
            detectEncodingFromByteOrderMarks: true, bufferSize: 65536, leaveOpen: leaveOpen);
        _ownsStream = !leaveOpen;
    }

    public long Position => _reader.BaseStream.Position;
    public long Length => _reader.BaseStream.Length;
    public double ProgressPercent => Length > 0 ? (100.0 * Position / Length) : 0;

    /// <summary>
    /// Streams games with inline ELO filtering - fastest option.
    /// Only parses WhiteElo and BlackElo tags, returns raw game text.
    /// No move parsing, minimal allocations.
    /// </summary>
    public IEnumerable<GameTextWithElo> StreamWithEloFilter(int minElo)
    {
        return StreamWithEloFilter(minElo, skipUnfinishedGames: true);
    }

    /// <summary>
    /// Streams games with inline ELO and result filtering.
    /// Filters out unfinished games (Result="*") and games with problematic terminations.
    /// </summary>
    public IEnumerable<GameTextWithElo> StreamWithEloFilter(int minElo, bool skipUnfinishedGames)
    {
        var gameText = new System.Text.StringBuilder(4096);
        int whiteElo = 0;
        int blackElo = 0;
        string result = null;
        string termination = null;
        bool hasGameStarted = false;

        while (!_reader.EndOfStream)
        {
            var line = _reader.ReadLine();
            if (line == null) break;

            var trimmedLine = line.Trim();

            // Empty line
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                if (hasGameStarted && gameText.Length > 0)
                {
                    gameText.AppendLine();
                }
                continue;
            }

            // Check for new game (Event tag typically first)
            if (trimmedLine.StartsWith("[Event ", StringComparison.OrdinalIgnoreCase))
            {
                // Yield previous game if it meets all criteria and has content
                if (hasGameStarted && gameText.Length > 0)
                {
                    int minPlayerElo = Math.Min(whiteElo, blackElo);
                    if (minPlayerElo >= minElo)
                    {
                        // Check if game is finished
                        bool isFinished = !skipUnfinishedGames || IsGameFinished(result, termination);

                        if (isFinished)
                        {
                            var completeGameText = gameText.ToString().Trim();

                            // Skip games without moves
                            if (completeGameText.Contains("1.") || completeGameText.Contains("1 "))
                            {
                                yield return new GameTextWithElo
                                {
                                    GameText = completeGameText,
                                    WhiteElo = whiteElo,
                                    BlackElo = blackElo,
                                    MinElo = minPlayerElo,
                                    Result = result,
                                    Termination = termination,
                                    FilePosition = Position
                                };
                            }
                        }
                    }

                    // Reset for next game
                    gameText.Clear();
                    whiteElo = 0;
                    blackElo = 0;
                    result = null;
                    termination = null;
                }

                hasGameStarted = true;
                gameText.AppendLine(line); // Add the new Event tag to start new game
                continue; // Skip the rest and continue to next line
            }

            // Fast tag extraction without full tag parsing
            if (hasGameStarted)
            {
                if (trimmedLine.StartsWith("[WhiteElo ", StringComparison.OrdinalIgnoreCase))
                {
                    whiteElo = ExtractEloFromTagLine(trimmedLine);
                }
                else if (trimmedLine.StartsWith("[BlackElo ", StringComparison.OrdinalIgnoreCase))
                {
                    blackElo = ExtractEloFromTagLine(trimmedLine);
                }
                else if (trimmedLine.StartsWith("[Result ", StringComparison.OrdinalIgnoreCase))
                {
                    result = ExtractTagValue(trimmedLine);
                }
                else if (trimmedLine.StartsWith("[Termination ", StringComparison.OrdinalIgnoreCase))
                {
                    termination = ExtractTagValue(trimmedLine);
                }

                gameText.AppendLine(line);
            }
        }

        // Yield last game
        if (hasGameStarted && gameText.Length > 0)
        {
            int minPlayerElo = Math.Min(whiteElo, blackElo);
            if (minPlayerElo >= minElo)
            {
                bool isFinished = !skipUnfinishedGames || IsGameFinished(result, termination);

                if (isFinished)
                {
                    yield return new GameTextWithElo
                    {
                        GameText = gameText.ToString(),
                        WhiteElo = whiteElo,
                        BlackElo = blackElo,
                        MinElo = minPlayerElo,
                        Result = result,
                        Termination = termination,
                        FilePosition = Position
                    };
                }
            }
        }
    }

    /// <summary>
    /// Fast ELO extraction from tag line like [WhiteElo "2500"]
    /// Avoids full tag parsing for performance.
    /// </summary>
    private static int ExtractEloFromTagLine(string line)
    {
        // Find the quoted value
        int firstQuote = line.IndexOf('"');
        if (firstQuote < 0) return 0;

        int secondQuote = line.IndexOf('"', firstQuote + 1);
        if (secondQuote < 0) return 0;

        var eloStr = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);

        if (int.TryParse(eloStr, out int elo))
        {
            return elo;
        }

        return 0;
    }

    /// <summary>
    /// Extract tag value from tag line like [Result "1-0"]
    /// </summary>
    private static string ExtractTagValue(string line)
    {
        int firstQuote = line.IndexOf('"');
        if (firstQuote < 0) return null;

        int secondQuote = line.IndexOf('"', firstQuote + 1);
        if (secondQuote < 0) return null;

        return line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
    }

    /// <summary>
    /// Checks if a game is finished based on Result and Termination tags.
    /// Returns false for unfinished games (Result="*" or problematic terminations).
    /// </summary>
    private static bool IsGameFinished(string result, string termination)
    {
        // If result is "*", the game is not finished
        if (result == "*")
        {
            return false;
        }

        // Check for problematic termination values that indicate unfinished games
        if (!string.IsNullOrEmpty(termination))
        {
            var lowerTermination = termination.ToLowerInvariant();

            // Skip games that were abandoned, unterminated, or have unknown status
            if (lowerTermination.Contains("unterminated") ||
                lowerTermination.Contains("abandoned") ||
                lowerTermination.Contains("unknown") ||
                lowerTermination.Contains("rules infraction"))
            {
                return false;
            }
        }

        // If result is valid (1-0, 0-1, 1/2-1/2) or empty but termination is valid
        return true;
    }

    /// <summary>
    /// Collects ELO statistics for optimal threshold analysis.
    /// Very fast - only extracts ELO tags, skips everything else.
    /// </summary>
    public IEnumerable<EloStatistic> CollectEloStatistics()
    {
        int whiteElo = 0;
        int blackElo = 0;
        bool hasGameStarted = false;
        long gamePosition = 0;

        while (!_reader.EndOfStream)
        {
            var line = _reader.ReadLine();
            if (line == null) break;

            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            // New game starting
            if (trimmedLine.StartsWith("[Event ", StringComparison.OrdinalIgnoreCase))
            {
                // Yield previous game stats
                if (hasGameStarted)
                {
                    yield return new EloStatistic
                    {
                        WhiteElo = whiteElo,
                        BlackElo = blackElo,
                        MinElo = Math.Min(whiteElo, blackElo),
                        MaxElo = Math.Max(whiteElo, blackElo),
                        FilePosition = gamePosition
                    };

                    whiteElo = 0;
                    blackElo = 0;
                }

                hasGameStarted = true;
                gamePosition = Position;
            }
            else if (hasGameStarted)
            {
                // Fast ELO extraction
                if (trimmedLine.StartsWith("[WhiteElo ", StringComparison.OrdinalIgnoreCase))
                {
                    whiteElo = ExtractEloFromTagLine(trimmedLine);
                }
                else if (trimmedLine.StartsWith("[BlackElo ", StringComparison.OrdinalIgnoreCase))
                {
                    blackElo = ExtractEloFromTagLine(trimmedLine);
                }
            }
        }

        // Yield last game
        if (hasGameStarted)
        {
            yield return new EloStatistic
            {
                WhiteElo = whiteElo,
                BlackElo = blackElo,
                MinElo = Math.Min(whiteElo, blackElo),
                MaxElo = Math.Max(whiteElo, blackElo),
                FilePosition = gamePosition
            };
        }
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

/// <summary>
/// Game text with extracted ELO ratings for fast filtering.
/// </summary>
public class GameTextWithElo
{
    public string GameText { get; set; }
    public int WhiteElo { get; set; }
    public int BlackElo { get; set; }
    public int MinElo { get; set; }
    public string Result { get; set; }
    public string Termination { get; set; }
    public long FilePosition { get; set; }
}

/// <summary>
/// Lightweight ELO statistics for threshold analysis.
/// </summary>
public class EloStatistic
{
    public int WhiteElo { get; set; }
    public int BlackElo { get; set; }
    public int MinElo { get; set; }
    public int MaxElo { get; set; }
    public long FilePosition { get; set; }
}
