using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using Newtonsoft.Json;
using StockfishApp.Core;
using StockfishApp.Models;
using StockFishCore;
using StockFishCore.Models;
using System.Diagnostics;
using Tools.Common;

namespace StockfishApp
{
    internal class StockFishGame
    {
        private Dictionary<StrategyType, string> _strategyTypeMap = new Dictionary<StrategyType, string> 
        {
            {StrategyType.NegaMax,"ab"},
            {StrategyType.LMR,"lmr"},
            {StrategyType.LMRD,"lmrd"},
            {StrategyType.ID,"id"},
            {StrategyType.ASP,"lmrd_asp"},
        };

        private StrategyBase _endGameTestStrategy;
        public StockFishGame(short depth, short stDepth, string game, string color, int elo, List<MoveBase> moves)
        {
            Depth = depth;
            StDepth = stDepth; 
            
            Stockfish = new Stockfish(@"..\..\..\stockfish\stockfish-windows-x86-64-avx2.exe", stDepth, elo);

            Position = new Position();

            Move = moves;

            foreach (var move in Move)
            {
                AddMove(move);
            }

            IStrategyFactory strategyFactory = Boot.GetService<IStrategyFactory>();

            Strategy = strategyFactory.GetStrategy(depth, Position, game);

            _endGameTestStrategy = strategyFactory.GetStrategy(2, Position, "ab");

            Color = color;

            Elo = elo;
        }

        public int Count { get; set; }
        public int Elo { get; private set; }
        public short Depth { get; }
        public short StDepth { get; }
        public Stockfish Stockfish { get; set; }
        public Position Position { get; set; }
        public StrategyBase Strategy { get; set; }
        public string Color { get; private set; }
        public List<MoveBase> Move { get; private set; }
        public string Opening { get; private set; }
        public string Error { get; private set; }

        internal StockFishGameResult Play()
        {
            try
            {
                var timer = Stopwatch.StartNew();

                var isStockfishMove = (Color == "w" && Position.GetTurn() == Turn.White) || (Color == "b" && Position.GetTurn() == Turn.Black);
                IResult result = new Result();

                FullMoves fullMoves = new FullMoves();
                while (result.GameResult == GameResult.Continue)
                {
                    if (isStockfishMove)
                    {
                        Stockfish.SetPosition(Position.GetHistory().Select(m => m.ToUciString()).ToArray());
                        var bestMove = Stockfish.GetBestMove();
                        var moves = Position.GetAllMoves();

                        MoveBase move = null;

                        foreach (var m in moves)
                        {
                            var uci = m.ToUciString();

                            if (uci == bestMove)
                            {
                                move = m;
                                break;
                            }
                        }

                        // position.GetAllMoves

                        if (move == null)
                        {
                            if (moves == null || moves.Count == 0)
                            {
                                result = _endGameTestStrategy.GetResult(short.MinValue, short.MaxValue, 1);
                            }
                            else
                            {
                                Debugger.Launch();
                            }
                        }
                        else
                        {
                            AddMove(move);
                        }
                    }
                    else
                    {
                        result = Strategy.GetResult();

                        if (result.Move != null)
                        {

                            AddMove(result.Move);
                        }
                    }

                    isStockfishMove = !isStockfishMove;
                }

                StockFishGameResult StockFishGameResult = new StockFishGameResult(Depth, StDepth, Strategy, Color, Elo, Move);

                if (result.GameResult == GameResult.Mate)
                {
                    if (Position.GetTurn() == Engine.Models.Enums.Turn.White)
                    {
                        StockFishGameResult.Output = StockFishGameResultType.Black;
                    }
                    else
                    {
                        StockFishGameResult.Output = StockFishGameResultType.White;
                    }
                }
                else
                {
                    StockFishGameResult.Output = StockFishGameResultType.Draw;
                }

                timer.Stop();

                StockFishGameResult.History = Position.GetHistory().ToList();
                StockFishGameResult.Moves = fullMoves;
                StockFishGameResult.Value = -Position.GetValue();
                StockFishGameResult.Static = -Position.GetStaticValue();
                Stockfish.SetPosition(Position.GetHistory().Select(m => m.ToUciString()).ToArray());
                StockFishGameResult.Board = Stockfish.GetBoardVisual();
                StockFishGameResult.Time = timer.Elapsed;

                return StockFishGameResult;
            }
            catch (Exception e)
            {
                StockFishLog log = new StockFishLog
                {
                    Color = Color,
                    Elo = Elo,
                    Ply = Boot.GetService<MoveHistoryService>().GetPly(),
                    Depth = Depth,
                    StDepth = StDepth,
                    Strategy = _strategyTypeMap[Strategy.Type],
                    History = Position.GetHistory().Select(m=>m.Key).ToArray(),
                    Opening = Move.Select(m => m.Key).ToArray(),
                    Error = e.ToFormattedString()
                };

                var json = JsonConvert.SerializeObject(log, Formatting.Indented);

                File.WriteAllText(Path.Combine("Log",$"{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_ffff")}.json"), json );

                throw;
            }
        }

        private void AddMove(MoveBase move)
        {
            //fullMoves.Add(move);
            if (Position.GetHistory().Any())
            {
                Position.Make(move);
            }
            else
            {
                Position.MakeFirst(move);
            }

            if (Position.GetTurn() == Engine.Models.Enums.Turn.White)
            {
                //Console.WriteLine($"{move}. V = {Position.GetValue()}, S = {Position.GetStaticValue()}"); 
                Count++;
            }
            else
            {
                //Console.WriteLine($"{Count} {elapsed}");
            }
        }
    }
}
