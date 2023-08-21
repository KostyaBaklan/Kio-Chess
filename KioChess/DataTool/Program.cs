﻿using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
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

        var depths = new List<short> { 3, 4, 5, 6, 7, 8, 4, 5, 6, 7, 8, 9, 5, 6, 7, 8, 6, 7, 8, 9, 5, 6, 7, 8, 10, 7, 4, 5, 6 };

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

        var list = JsonConvert.DeserializeObject<List<MoveSequence>>(File.ReadAllText("Config\\Seuquence.json"));

        foreach (var move in list)
        {
            string moveSequence = move.GetSequence(moveProvider);

            Console.WriteLine(moveSequence);
        }

        MoveSequence ms = list.GetRandomItem();

        //var firstMoves = GenerateFirstMoves(moveProvider);

        //var firstMove = firstMoves.GetRandomItem();

        //position.MakeFirst(firstMove);

        //var secondMoves = GenerateSecondMoves(moveProvider);
        //var secondMove = secondMoves.GetRandomItem();

        //position.Make(secondMove);

        position.MakeFirst(moveProvider.Get(ms.M1));
        position.Make(moveProvider.Get(ms.M2));
        position.Make(moveProvider.Get(ms.M3));
        position.Make(moveProvider.Get(ms.M4));

        GameResult gameResult = GameResult.Continue;

        StrategyBase strategy = blackStrategy;

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
        for(short i = 14589; i <= 14609; i++) { excluded.Add(i); }

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

    private static List<MoveBase> GenerateAllMoves(IPosition position)
    {
        List<MoveBase> moves = new List<MoveBase>();

        foreach (var p in new List<byte> { Pieces.WhiteKnight })
        {
            foreach (var s in new List<byte> { Squares.B1, Squares.G1 })
            {
                var all = position.GetAllMoves(s, p);
                moves.AddRange(all);
            }
        }

        foreach (var p in new List<byte> { Pieces.WhitePawn })
        {
            foreach (var s in new List<byte> { Squares.A2, Squares.B2, Squares.C2, Squares.D2, Squares.E2, Squares.F2, Squares.G2, Squares.H2 })
            {
                var all = position.GetAllMoves(s, p);
                moves.AddRange(all);
            }
        }

        return moves;
    }

    private static List<MoveBase> GenerateFirstMoves(IMoveProvider provider)
    {
        List<MoveBase> moves = new List<MoveBase>();

        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.B2).FirstOrDefault(m => m.To == Squares.B3));
        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.C2).FirstOrDefault(m => m.To == Squares.C3));
        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.C2).FirstOrDefault(m => m.To == Squares.C4));
        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.D2).FirstOrDefault(m => m.To == Squares.D4));
        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.D2).FirstOrDefault(m => m.To == Squares.D3));
        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E3));
        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E4));
        moves.Add(provider.GetMoves(Pieces.WhitePawn, Squares.G2).FirstOrDefault(m => m.To == Squares.G3));
        moves.Add(provider.GetMoves(Pieces.WhiteKnight, Squares.B1).FirstOrDefault(m => m.To == Squares.C3));
        moves.Add(provider.GetMoves(Pieces.WhiteKnight, Squares.G1).FirstOrDefault(m => m.To == Squares.F3));

        return moves;
    }

    private static List<MoveBase> GenerateSecondMoves(IMoveProvider provider)
    {
        List<MoveBase> moves = new List<MoveBase>();

        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.B7).FirstOrDefault(m => m.To == Squares.B6));
        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.C7).FirstOrDefault(m => m.To == Squares.C6));
        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.C7).FirstOrDefault(m => m.To == Squares.C5));
        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.D7).FirstOrDefault(m => m.To == Squares.D6));
        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.D7).FirstOrDefault(m => m.To == Squares.D5));
        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.E7).FirstOrDefault(m => m.To == Squares.E6));
        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.E7).FirstOrDefault(m => m.To == Squares.E5));
        moves.Add(provider.GetMoves(Pieces.BlackPawn, Squares.G7).FirstOrDefault(m => m.To == Squares.G6));
        moves.Add(provider.GetMoves(Pieces.BlackKnight, Squares.B8).FirstOrDefault(m => m.To == Squares.C6));
        moves.Add(provider.GetMoves(Pieces.BlackKnight, Squares.G8).FirstOrDefault(m => m.To == Squares.F6));

        return moves;
    }
}