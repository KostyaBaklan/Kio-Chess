using DataAccess.Entities;
using Engine.Communication.Client;
using Engine.Dal.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Pgn;
using Engine.Pgn.Models;
using GamesServices;
using ProtoBuf;
using Tools.Common;

namespace PgnTool;

internal class Program
{
    private static int _depth;
    private static IServiceClient<ISequenceService> _serviceClient = null!;
    private static IGameDbService _gameDbService = null!;

    private static async Task Main(string[] args)
    {
        Boot.SetUp();

        SequenceClient client = new SequenceClient();
        _serviceClient = client.GetClient();

        _gameDbService = Boot.GetService<IGameDbService>();

        try
        {
            _depth = Boot.GetService<IConfigurationProvider>().BookConfiguration.SaveDepth;

            //var dir = @"C:\Projects\AI\Kio-Chess\KioChess\Data\Release\net8.0\PGNs\Failures";

            //var file = Path.Combine(dir, "PGN_Failures_2023_09_20_02_38_24_2431_37225d10-2a19-4c2a-8712-0a38596072e6.pgn");

            //ProcessFile(file);

            await ProcessArgumentAsync(args);
        }
        finally
        {
            await client.CloseAsync();
        }
    }

    private static async Task ProcessFileAsync(string fileName)
    {
        FileInfo file = new FileInfo(fileName);
        try
        {
            PgnReader pgnReader = new PgnReader();
            PgnDatabase database = pgnReader.ReadFromFile(file.FullName);

            var game = database.Games.FirstOrDefault();

            await ProcessGameAsync(game);

            Console.WriteLine("No failure !!!");

            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to delete '{file.FullName}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(file.FullName);
            Console.WriteLine(ex.ToFormattedString());
        }
    }

    private static async Task ProcessArgumentAsync(string[] args)
    {
        PgnDatabase database = null;
        try
        {
            PgnReader pgnReader = new PgnReader();
            database = pgnReader.ReadFromString(args[0]);

            var game = database.Games.FirstOrDefault();

            if (game == null)
            {
                return;
            }

            await ProcessGameAsync(game);
        }
        catch (Exception ex)
        {
            if (database == null)
            {
                return;
            }

            Console.WriteLine(ex.ToFormattedString());
            PgnWriter pgnWriter = new PgnWriter(@$"PGNs\Failures\PGN_Failures_{DateTime.Now.ToFileName()}_{Guid.NewGuid()}.pgn");
            pgnWriter.Write(database);
        }
    }

    private static async Task ProcessGameAsync(PgnGame game)
    {
        Position position = new Position();

        var ms = game.Moves.Take(_depth).ToList();
        var result = game.Result;

        if (result == GameResult.None && game.Tags.TryGetValue("Result", out var resultStr))
        {
            result = PgnConverter.ConvertResult(resultStr);
        }

        if (ms != null && result != GameResult.None && ms.Any())
        {
            bool isWhite = true;
            foreach (var m in ms)
            {
                PgnConverter.ProcessMove(position, m, isWhite);
                isWhite = !isWhite;
            }

            await ProcessEndGameAsync(result);
        }
        else
        {
            throw new Exception("No Moves or End Game!");
        }
    }

    private static async Task ProcessEndGameAsync(GameResult result)
    {
        List<Book> records = result switch
        {
            GameResult.White => _gameDbService.CreateRecords(1, 0, 0),
            GameResult.Black => _gameDbService.CreateRecords(0, 0, 1),
            _ => _gameDbService.CreateRecords(0, 1, 0),
        };

        using (var ms = new MemoryStream())
        {
            Serializer.Serialize(ms, records);
            await _serviceClient.CallAsync("ProcessSequence", ms.ToArray());
        }
    }
}
