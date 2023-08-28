using Data.Common;
using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;
using Engine.Strategies.ID;
using Engine.Strategies.Lmr;
using Engine.Strategies.Null;
using Newtonsoft.Json;
using System.Diagnostics;
using Tools.Common;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        IDataAccessService dataAccessService = Boot.GetService<IDataAccessService>();

        dataAccessService.Connect();

        var task = dataAccessService.LoadAsync(Boot.GetService<IBookService>());

        var strategies = new List<string> { "ab_null", "lmr_null", "lmr_null", "lmrd_null", "lmrd_null", "lmrd_null", "id", "id", "id", "id", "id", "lmrd", "lmrd", "lmrd", "lmrd", "lmrd", "lmrd_asp", "lmrd_asp", "lmrd_asp", "lmr_asp", "lmr_asp", "lmr" };

        var depths = new List<short> { 3, 4, 4, 5, 5, 5, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 9, 9, 10 };

        Dictionary<string, Func<short, IPosition, StrategyBase>> strategyFactories =
                new Dictionary<string, Func<short, IPosition, StrategyBase>>
                {
                    {"lmr", (d, p) => new LmrStrategy(d, p)},
                    {"lmrd", (d, p) => new LmrDeepStrategy(d, p)},
                    {"lmr_asp", (d, p) => new LmrAspirationStrategy(d, p)},
                    {"lmrd_asp", (d, p) => new LmrDeepAspirationStrategy(d, p)},
                    {"id", (d, p) => new IteretiveDeepingStrategy(d, p)},
                    {"ab_null", (d, p) => new NullNegaMaxMemoryStrategy(d, p)},
                    {"lmr_null", (d, p) => new NullLmrStrategy(d, p)},
                    {"lmrd_null", (d, p) => new NullLmrDeepStrategy(d, p)}
                };

        var position = new Position();

        var moveProvider = Boot.GetService<IMoveProvider>();

        StrategyBase whiteStrategy = strategyFactories[strategies.GetRandomItem()](depths.GetRandomItem(), position);

        StrategyBase blackStrategy = strategyFactories[strategies.GetRandomItem()](depths.GetRandomItem(), position);

        var list = File.ReadLines("Config\\Sequence.txt")
            .Select(JsonConvert.DeserializeObject<MoveSequence>)
            .ToList();

        MoveSequence ms = list.GetRandomItem();

        position.MakeFirst(moveProvider.Get(ms.Keys[0]));
        for (int i = 1; i < ms.Keys.Count; i++)
        {
            position.Make(moveProvider.Get(ms.Keys[i]));
        }

        GameResult gameResult = GameResult.Continue;

        int historyCount = position.GetHistory().Count();

        StrategyBase strategy = historyCount % 2 == 0 ? blackStrategy : whiteStrategy;

        try
        {
            while (gameResult == GameResult.Continue)
            {
                if (strategy == whiteStrategy)
                {
                    strategy = blackStrategy;
                }
                else
                {
                    strategy = whiteStrategy;
                }

                task.Wait();

                dataAccessService.Disconnect();

                var result = strategy.GetResult();

                gameResult = result.GameResult;

                if (gameResult == GameResult.Continue)
                {
                    position.Make(result.Move);
                }
            }
        }
        catch (Exception ex)
        {
            var error = ex.ToFormattedString();

            Console.WriteLine(error);

            throw new ApplicationException("Crash", ex);
        }

        try
        {
            dataAccessService.Connect();

            timer.Stop();

            string generalMessage = $"Time = {timer.Elapsed}. [{gameResult}]. Moves = {Boot.GetService<IMoveHistoryService>().GetPly()}";

            if (gameResult == GameResult.Mate)
            {
                if (strategy == whiteStrategy)
                {
                    Console.WriteLine($"Black Win -> {whiteStrategy} < {blackStrategy}. {generalMessage}");
                    dataAccessService.UpdateHistory(GameValue.BlackWin);
                }
                else
                {
                    Console.WriteLine($"White Win -> {whiteStrategy} > {blackStrategy}. {generalMessage}");
                    dataAccessService.UpdateHistory(GameValue.WhiteWin);
                }
            }
            else
            {
                Console.WriteLine($"Draw -> {whiteStrategy} = {blackStrategy}. {generalMessage}");
                dataAccessService.UpdateHistory(GameValue.Draw);
            }
        }
        finally
        {
            dataAccessService.Disconnect();
        }
    }
}