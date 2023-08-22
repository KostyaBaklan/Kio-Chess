using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Sorting.Comparers;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;
using Engine.Strategies.ID;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models;
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

        var strategies = new List<string> { "lmrd", "lmr", "lmr_asp", "lmrd_asp", "ab_null", "lmr_null", "lmrd_null", "lmrd",
            "id", "lmr_asp", "lmrd_asp", "lmr_null", "id", "lmrd_null", "lmrd", "lmr_asp", "lmrd_asp", "id",
            "lmrd_null", "lmrd", "lmr_asp", "lmrd_asp","lmr_null","lmrd", "lmr", "lmr_asp", "lmrd_asp", "id", "id" };

        var depths = new List<short> { 3, 4, 5, 6, 7, 8, 4, 5, 6, 7, 8, 9, 5, 6, 7, 8, 6, 7, 8, 9, 5, 6, 7, 8, 10, 3, 4, 5, 6, 7 };

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

        //UpdateSequence(position, moveProvider);

        List<MoveSequence> list = JsonConvert.DeserializeObject<List<MoveSequence>>(File.ReadAllText("Config\\Seuquence.json"));

        MoveSequence ms = list.GetRandomItem();

        position.MakeFirst(moveProvider.Get(ms.M1));
        position.Make(moveProvider.Get(ms.M2));
        position.Make(moveProvider.Get(ms.M3));
        position.Make(moveProvider.Get(ms.M4));

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

            var history = position.GetHistory();

            timer.Stop();

            string generalMessage = $"Time = {timer.Elapsed}. [{gameResult}]. Moves = {Boot.GetService<IMoveHistoryService>().GetPly()}";

            if (gameResult == GameResult.Mate)
            {
                if (strategy == whiteStrategy)
                {
                    Console.WriteLine($"Black Win -> {whiteStrategy} < {blackStrategy}. {generalMessage}");
                    dataAccessService.AddHistory(history, GameValue.BlackWin);
                }
                else
                {
                    Console.WriteLine($"White Win -> {whiteStrategy} > {blackStrategy}. {generalMessage}");
                    dataAccessService.AddHistory(history, GameValue.WhiteWin);
                }
            }
            else
            {
                Console.WriteLine($"Draw -> {whiteStrategy} = {blackStrategy}. {generalMessage}");
                dataAccessService.AddHistory(history, GameValue.Draw);
            }
        }
        finally
        {
            dataAccessService.Disconnect();
        }
    }

    private static void UpdateSequence(Position position, IMoveProvider moveProvider)
    {
        List<MoveSequence> list = new List<MoveSequence>();

        //HashSet<short> excluded = new HashSet<short> { 7695, 9589, 7687 };
        HashSet<short> excluded = new HashSet<short> { 11431, 11441, 11445 };
        for (short i = 14589; i <= 14609; i++) { excluded.Add(i); }

        HashSet<short> excludedM3 = new HashSet<short> { 7681, 7683, 7685, 7687, 7689, 7691, 7693, 7695 };

        foreach (var sequence in JsonConvert.DeserializeObject<List<MoveSequence>>(File.ReadAllText("Config\\Seuquence.json")))
        {
            if (excludedM3.Contains(sequence.M3)) continue;

            position.MakeFirst(moveProvider.Get(sequence.M1));
            position.Make(moveProvider.Get(sequence.M2));
            position.Make(moveProvider.Get(sequence.M3));

            SortContext sortContext = Boot.GetService<IDataPoolService>().GetCurrentSortContext();
            sortContext.Set(Boot.GetService<IMoveSorterProvider>().GetComplex(position, new HistoryComparer()), null);

            var ml = position.GetAllMoves(sortContext);
            var moves = ml.Where(z => !excluded.Contains(z.Key)).Take(4).ToList();

            foreach (var move in moves)
            {
                var l = new MoveSequence { M1 = sequence.M1, M2 = sequence.M2, M3 = sequence.M3, M4 = move.Key };

                list.Add(l);
            }

            for (int i = 0; i < 3; i++)
            {
                position.UnMake();
            }
        }

        var json = JsonConvert.SerializeObject(list, Formatting.Indented);

        File.WriteAllText("Seuquence.json", json);
    }
}