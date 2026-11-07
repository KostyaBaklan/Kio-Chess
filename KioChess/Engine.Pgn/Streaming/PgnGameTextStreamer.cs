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
    private readonly Stream _progressStream;
    private bool _disposed;

    public PgnGameTextStreamer(string filePath)
    {
        var stream = PgnFiles.Open(filePath, out var raw);
        _progressStream = raw;
        _reader = new StreamReader(stream, System.Text.Encoding.UTF8, 
            detectEncodingFromByteOrderMarks: true, bufferSize: 65536);
        _ownsStream = true;
    }

    public PgnGameTextStreamer(Stream stream, bool leaveOpen = false)
    {
        _progressStream = stream;
        _reader = new StreamReader(stream, System.Text.Encoding.UTF8, 
            detectEncodingFromByteOrderMarks: true, bufferSize: 65536, leaveOpen: leaveOpen);
        _ownsStream = !leaveOpen;
    }

    public long Position => _progressStream.Position;
    public long Length => _progressStream.Length;
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
    /// Number of games seen so far (accepted or not) by <see cref="StreamFiltered"/>.
    /// </summary>
    public long GamesRead { get; private set; }

    /// <summary>
    /// Streams games accepted by the time-control / Elo filter. Only tags are inspected; no move parsing.
    /// </summary>
    public IEnumerable<GameTextWithElo> StreamFiltered(PgnGameFilter filter, bool skipUnfinishedGames = true)
    {
        var gameText = new System.Text.StringBuilder(4096);
        int whiteElo = 0;
        int blackElo = 0;
        string result = null;
        string termination = null;
        string timeControl = null;
        bool hasGameStarted = false;

        while (!_reader.EndOfStream)
        {
            var line = _reader.ReadLine();
            if (line == null) break;

            if (string.IsNullOrWhiteSpace(line))
            {
                if (hasGameStarted && gameText.Length > 0) gameText.AppendLine();
                continue;
            }

            if (line.StartsWith("[Event ", StringComparison.Ordinal))
            {
                if (hasGameStarted)
                {
                    GamesRead++;
                    var game = BuildFilteredGame(filter, skipUnfinishedGames, gameText, whiteElo, blackElo, result, termination, timeControl);
                    if (game != null) yield return game;
                }

                gameText.Clear();
                whiteElo = 0;
                blackElo = 0;
                result = null;
                termination = null;
                timeControl = null;
                hasGameStarted = true;
                gameText.AppendLine(line);
                continue;
            }

            if (!hasGameStarted) continue;

            if (line.StartsWith("[WhiteElo ", StringComparison.Ordinal)) whiteElo = ExtractEloFromTagLine(line);
            else if (line.StartsWith("[BlackElo ", StringComparison.Ordinal)) blackElo = ExtractEloFromTagLine(line);
            else if (line.StartsWith("[Result ", StringComparison.Ordinal)) result = ExtractTagValue(line);
            else if (line.StartsWith("[Termination ", StringComparison.Ordinal)) termination = ExtractTagValue(line);
            else if (line.StartsWith("[TimeControl ", StringComparison.Ordinal)) timeControl = ExtractTagValue(line);

            gameText.AppendLine(line);
        }

        if (hasGameStarted)
        {
            GamesRead++;
            var game = BuildFilteredGame(filter, skipUnfinishedGames, gameText, whiteElo, blackElo, result, termination, timeControl);
            if (game != null) yield return game;
        }
    }

    private GameTextWithElo BuildFilteredGame(PgnGameFilter filter, bool skipUnfinishedGames, System.Text.StringBuilder gameText,
        int whiteElo, int blackElo, string result, string termination, string timeControl)
    {
        if (gameText.Length == 0) return null;

        int minPlayerElo = Math.Min(whiteElo, blackElo);
        if (!filter.Accepts(minPlayerElo, timeControl)) return null;
        if (skipUnfinishedGames && !IsGameFinished(result, termination)) return null;

        var text = gameText.ToString().Trim();
        if (!text.Contains("1.") && !text.Contains("1 ")) return null;

        return new GameTextWithElo
        {
            GameText = text,
            WhiteElo = whiteElo,
            BlackElo = blackElo,
            MinElo = minPlayerElo,
            Result = result,
            Termination = termination,
            FilePosition = Position
        };
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
        string timeControl = null;
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
                        FilePosition = gamePosition,
                        TimeControl = timeControl
                    };

                    whiteElo = 0;
                    blackElo = 0;
                    timeControl = null;
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
                else if (trimmedLine.StartsWith("[TimeControl ", StringComparison.OrdinalIgnoreCase))
                {
                    timeControl = ExtractTagValue(trimmedLine);
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
                FilePosition = gamePosition,
                TimeControl = timeControl
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
                _progressStream?.Dispose();
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
    public string TimeControl { get; set; }
}
