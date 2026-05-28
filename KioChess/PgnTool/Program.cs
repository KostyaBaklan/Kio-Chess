using Engine.Communication.Client;
using Engine.Dal.Services;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Hash;
using Engine.Pgn;
using Engine.Pgn.Models;
using Engine.Services;
using GamesServices;
using ProtoBuf;
using Tools.Common;

namespace PgnTool;

internal class Program
{
    private static int _depth;
    private static IServiceClient<ISequenceService> _serviceClient = null!;
    private static MoveHistoryService _moveHistoryService = null!;
    private static GameEntityFactory _gameEntityFactory = null!;

    private static async Task Main(string[] args)
    {
        Boot.SetUp();

        SequenceClient client = new SequenceClient();
        _serviceClient = client.GetClient();

        _moveHistoryService = Boot.GetService<MoveHistoryService>();

        try
        {
            _depth = Boot.GetService<IConfigurationProvider>().BookConfiguration.SaveDepth;

            // Initialize GameEntityFactory for new 128-bit hash pipeline
            _gameEntityFactory = Boot.GetService<GameEntityFactory>();

            // Initialize MoveHashSequenceHasher if not already initialized
            if (!MoveHashSequenceHasher.IsInitialized)
            {
                var appDbService = Boot.GetService<DataAccess.Interfaces.IAppDbService>();
                appDbService.Connect();
                var moveHashes = appDbService.GetAllMoveHashValues();
                MoveHashSequenceHasher.Initialize(moveHashes);
                appDbService.Disconnect();
            }

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
        var moveKeyList = _moveHistoryService.GetSaveSequence();

        // Determine game result statistics
        var (white, draw, black) = result switch
        {
            GameResult.White => (1, 0, 0),
            GameResult.Black => (0, 0, 1),
            _ => (0, 1, 0),  // Draw
        };

        // Create GameEntity records with 128-bit hash
        List<DataAccess.Entities.GameEntity> records = _gameEntityFactory.CreateRecords(
            moveKeyList,
            white,
            draw,
            black
        );

        // Serialize and send to SequenceService
        using (var ms = new MemoryStream())
        {
            Serializer.Serialize(ms, records);
            await _serviceClient.CallAsync("ProcessSequence", ms.ToArray());
        }
    }
}
