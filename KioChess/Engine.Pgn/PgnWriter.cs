using Engine.Pgn.Models;
using System.Text;

namespace Engine.Pgn;

public class PgnWriter
{
    private readonly string _outputPath;

    public PgnWriter(string outputPath)
    {
        _outputPath = outputPath;
    }

    public void Write(PgnDatabase database)
    {
        var directory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(_outputPath, false, Encoding.UTF8);
        
        foreach (var game in database.Games)
        {
            WriteGame(writer, game);
            writer.WriteLine();
        }
    }

    public void WriteGame(StreamWriter writer, PgnGame game)
    {
        foreach (var tag in game.Tags)
        {
            writer.WriteLine($"[{tag.Key} \"{tag.Value}\"]");
        }

        writer.WriteLine();

        if (game.Moves.Count > 0)
        {
            WriteMoves(writer, game.Moves);
            writer.Write(" ");
        }

        writer.WriteLine(game.ResultString);
    }

    private void WriteMoves(StreamWriter writer, List<PgnMove> moves)
    {
        int moveNumber = 1;
        bool isWhiteMove = true;

        for (int i = 0; i < moves.Count; i++)
        {
            if (isWhiteMove)
            {
                writer.Write($"{moveNumber}. ");
            }
            else if (i == 0)
            {
                writer.Write($"{moveNumber}... ");
            }

            writer.Write(moves[i].Notation);
            writer.Write(" ");

            if (!isWhiteMove)
            {
                moveNumber++;
            }

            isWhiteMove = !isWhiteMove;

            if ((i + 1) % 10 == 0)
            {
                writer.WriteLine();
            }
        }
    }

    public static string ToString(PgnDatabase database)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8);
        
        var pgnWriter = new PgnWriter(string.Empty);
        
        foreach (var game in database.Games)
        {
            pgnWriter.WriteGame(writer, game);
            writer.WriteLine();
        }
        
        writer.Flush();
        ms.Position = 0;
        
        using var reader = new StreamReader(ms, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string ToString(PgnGame game)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8);
        
        var pgnWriter = new PgnWriter(string.Empty);
        pgnWriter.WriteGame(writer, game);
        
        writer.Flush();
        ms.Position = 0;
        
        using var reader = new StreamReader(ms, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
