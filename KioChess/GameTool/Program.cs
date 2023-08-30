using Data.Common;
using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Strategies.Null;
using GameTool.Strategies.Asp;
using GameTool.Strategies.Id;
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

        var strategies = new List<string>
        {
            "lmr", "lmr", "lmrd", "lmrd", "lmrd", "lmrd", "lmrd", "ab_null",
            "lmr_null", "lmrd_null", "lmr_null", "lmrd_null",
            "id1_lmr", "id1_lmrd", "id1_lmrd","id1_null", "id1_null_lmr", "id1_null_lmrd", "id1_null_lmrd",
            "id2_lmr", "id2_lmrd", "id2_lmrd","id2_null", "id2_null_lmr", "id2_null_lmrd","id2_null_lmrd",
            "asp1_lmr", "asp1_lmrd","asp1_lmrd", "asp1_null", "asp1_null_lmr", "asp1_null_lmrd","asp1_null_lmrd",
            "asp2_lmr", "asp2_lmrd",  "asp2_lmrd","asp2_null", "asp2_null_lmr", "asp2_null_lmrd", "asp2_null_lmrd",
            "id1_lmr", "id1_lmrd", "id1_lmrd","id1_null", "id1_null_lmr", "id1_null_lmrd", "id1_null_lmrd",
            "id2_lmr", "id2_lmrd", "id2_lmrd","id2_null", "id2_null_lmr", "id2_null_lmrd","id2_null_lmrd",
            "asp1_lmr", "asp1_lmrd","asp1_lmrd", "asp1_null", "asp1_null_lmr", "asp1_null_lmrd","asp1_null_lmrd",
            "asp2_lmr", "asp2_lmrd",  "asp2_lmrd","asp2_null", "asp2_null_lmr", "asp2_null_lmrd", "asp2_null_lmrd",
            "id1_lmr", "id1_lmrd", "id1_lmrd","id1_null", "id1_null_lmr", "id1_null_lmrd", "id1_null_lmrd",
            "id2_lmr", "id2_lmrd", "id2_lmrd","id2_null", "id2_null_lmr", "id2_null_lmrd","id2_null_lmrd",
            "asp1_lmr", "asp1_lmrd","asp1_lmrd", "asp1_null", "asp1_null_lmr", "asp1_null_lmrd","asp1_null_lmrd",
            "asp2_lmr", "asp2_lmrd",  "asp2_lmrd","asp2_null", "asp2_null_lmr", "asp2_null_lmrd", "asp2_null_lmrd"
        };

        var depths = new List<short> { 4, 4, 5, 5, 5, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 9, 9 };

        Dictionary<string, Func<short, IPosition, StrategyBase>> strategyFactories =
                new Dictionary<string, Func<short, IPosition, StrategyBase>>
                {
                    {"lmr", (d, p) => new LmrStrategy(d, p)},
                    {"lmrd", (d, p) => new LmrDeepStrategy(d, p)},
                    {"ab_null", (d, p) => new NullNegaMaxMemoryStrategy(d, p)},
                    {"lmr_null", (d, p) => new NullLmrStrategy(d, p)},
                    {"lmrd_null", (d, p) => new NullLmrDeepStrategy(d, p)},
                    {"id1_lmr", (d, p) => new LmrOneIdStrategy(d, p)},
                    {"id1_lmrd", (d, p) => new LmrDeepOneIdStrategy(d, p)},
                    {"id1_null", (d, p) => new NullOneIdStrategy(d, p)},
                    {"id1_null_lmr", (d, p) => new LmrNullOneIdStrategy(d, p)},
                    {"id1_null_lmrd", (d, p) => new LmrDeepNullOneIdStrategy(d, p)},
                    {"id2_lmr", (d, p) => new LmrTwoIdStrategy(d, p)},
                    {"id2_lmrd", (d, p) => new LmrDeepTwoIdStrategy(d, p)},
                    {"id2_null", (d, p) => new NullTwoIdStrategy(d, p)},
                    {"id2_null_lmr", (d, p) => new LmrNullTwoIdStrategy(d, p)},
                    {"id2_null_lmrd", (d, p) => new LmrDeepNullTwoIdStrategy(d, p)},
                    {"asp1_lmr", (d, p) => new LmrOneAspStrategy(d, p)},
                    {"asp1_lmrd", (d, p) => new LmrDeepOneAspStrategy(d, p)},
                    {"asp1_null", (d, p) => new NullOneAspStrategy(d, p)},
                    {"asp1_null_lmr", (d, p) => new LmrNullOneAspStrategy(d, p)},
                    {"asp1_null_lmrd", (d, p) => new LmrDeepNullOneAspStrategy(d, p)},
                    {"asp2_lmr", (d, p) => new LmrTwoAspStrategy(d, p)},
                    {"asp2_lmrd", (d, p) => new LmrDeepTwoAspStrategy(d, p)},
                    {"asp2_null", (d, p) => new NullTwoAspStrategy(d, p)},
                    {"asp2_null_lmr", (d, p) => new LmrNullTwoAspStrategy(d, p)},
                    {"asp2_null_lmrd", (d, p) => new LmrDeepNullTwoAspStrategy(d, p)}
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