using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using Newtonsoft.Json;
using StockfishApp.Core;
using StockfishApp.Exceptions;
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
            {StrategyType.ASP,"asp"},
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
                var fen = Stockfish.GetFenPosition();
                AddMove(fen, move);
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
            List<double> _moveTime = new List<double>();
            try
            {
                var timer = Stopwatch.StartNew();

                var isStockfishMove = (Color == "w" && Position.GetTurn() == Turn.White) || (Color == "b" && Position.GetTurn() == Turn.Black);
                IResult result = new Result();

                FullMoves fullMoves = new FullMoves();
                while (result.GameResult == GameResult.Continue)
                {
                    var fen = Stockfish.GetFenPosition();

                    if (isStockfishMove)
                    {
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

                        if (move == null)
                        {
                            if (moves == null || moves.Count == 0)
                            {
                                result = _endGameTestStrategy.GetResult(short.MinValue, short.MaxValue, 1);
                            }
                            else
                            {
                                var log = new StockFishMoveLog
                                {
                                    BestMove = bestMove,
                                    Moves = moves.Select(m => m.ToString()).ToList(),
                                    UCI = moves.Select(m => m.ToUciString()).ToList()
                                };

                                var json = JsonConvert.SerializeObject(log, Formatting.Indented);

                                File.WriteAllText(Path.Combine("Log", $"StockFishMoveLog_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_ffff")}.json"), json);

                                throw new NoMoveFoundException();
                            }
                        }
                        else
                        {
                            AddMove(fen, move);
                        }
                    }
                    else
                    {
                        var tr = Stopwatch.StartNew();
                        result = Strategy.GetResult();
                        tr.Stop();
                        _moveTime.Add(tr.ElapsedMilliseconds);

                        if (result.Move != null)
                        {
                            AddMove(fen, result.Move);
                        }
                    }

                    isStockfishMove = !isStockfishMove;
                }

                StockFishGameResult StockFishGameResult = new StockFishGameResult(Depth, StDepth, Strategy, Color, Elo, Move)
                {
                    OutputType = result.GameResult
                };

                if (result.GameResult == GameResult.Mate)
                {
                    if (Position.GetTurn() == Turn.White)
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
                StockFishGameResult.Time = timer.Elapsed.TotalMilliseconds;
                StockFishGameResult.MoveTime = _moveTime.Average();

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
                    History = Position.GetHistory().Select(m => m.Key).ToArray(),
                    Opening = Move.Select(m => m.Key).ToArray(),
                    Error = e.ToFormattedString()
                };

                var json = JsonConvert.SerializeObject(log, Formatting.Indented);

                File.WriteAllText(Path.Combine("Log", $"{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_ffff")}.json"), json);

                throw;
            }
        }

        private void AddMove(string fen, MoveBase move)
        {
            if (Position.GetHistory().Any())
            {
                Position.Make(move);
            }
            else
            {
                Position.MakeFirst(move);
            }

            if (Position.GetTurn() == Turn.White)
            {
                Count++;
            }

            Stockfish.SetPosition(fen, move.ToUciString());
        }
    }
}
