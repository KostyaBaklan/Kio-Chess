using Engine.Pgn.Models;
using Engine.Pgn.Parsers;
using System.Text;

namespace Engine.Pgn;

public class PgnReader
{
    public PgnDatabase ReadFromFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        return ReadFromString(content);
    }

    public PgnDatabase ReadFromString(string pgnContent)
    {
        var database = new PgnDatabase();
        var lines = pgnContent.Split('\n');
        
        PgnGame currentGame = null;
        StringBuilder moveTextBuilder = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (TagParser.IsTag(line))
            {
                if (currentGame != null && moveTextBuilder.Length > 0)
                {
                    ParseMoveText(currentGame, moveTextBuilder.ToString());
                    moveTextBuilder.Clear();
                }

                if (currentGame != null && currentGame.Tags.Count > 0 && currentGame.Moves.Count == 0)
                {
                    // Tag for current game
                }
                else
                {
                    if (currentGame != null)
                    {
                        database.AddGame(currentGame);
                    }
                    currentGame = new PgnGame();
                }

                if (TagParser.TryParseTag(line, out var tagName, out var tagValue))
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
                if (currentGame == null)
                {
                    currentGame = new PgnGame();
                }
                
                moveTextBuilder.AppendLine(line);
            }
        }

        if (currentGame != null)
        {
            if (moveTextBuilder.Length > 0)
            {
                ParseMoveText(currentGame, moveTextBuilder.ToString());
            }
            database.AddGame(currentGame);
        }

        return database;
    }

    private void ParseMoveText(PgnGame game, string moveText)
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
        var sb = new StringBuilder();
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

        for (int i = 0; i < moveText.Length; i++)
        {
            char c = moveText[i];
            
            if (char.IsWhiteSpace(c))
            {
                if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else if (c == '.' && sb.Length > 0 && sb.ToString().All(char.IsDigit))
            {
                // This is a move number like "1" followed by "."
                // Add it as a complete token "1."
                sb.Append('.');
                tokens.Add(sb.ToString());
                sb.Clear();
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
}
