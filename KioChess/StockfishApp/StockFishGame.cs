using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Strategies.Base;
using StockfishApp.Core;
using StockfishApp.Models;
using StockFishCore;
using System.Diagnostics;

namespace StockfishApp
{
    internal class StockFishGame
    {
        private StrategyBase _endGameTestStrategy;
        public StockFishGame(short depth, short stDepth, string game, string color, int elo, short moveKey)
        {
            Depth = depth;
            StDepth = stDepth; 
            
            Stockfish = new Stockfish(@"..\..\..\stockfish\stockfish-windows-x86-64-avx2.exe", stDepth, elo);

            Position = new Position(); 
            
            var moveProvider = Boot.GetService<MoveProvider>();

            var move = moveProvider.Get(moveKey);

            Move = move;
            Position.MakeFirst(move);

            IStrategyFactory strategyFactory = Boot.GetService<IStrategyFactory>();

            Strategy = strategyFactory.GetStrategy(depth, Position, game);

            _endGameTestStrategy = strategyFactory.GetStrategy(2, Position, "ab");

            Color = color;

            Count = 1;

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
        public MoveBase Move { get; private set; }


        internal StockFishGameResult Play()
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
                        AddMove(fullMoves, move, timer.Elapsed);
                    }
                }
                else
                {
                    result = Strategy.GetResult();

                    if (result.Move != null)
                    {

                        AddMove(fullMoves, result.Move, timer.Elapsed);
                    }
                }

                isStockfishMove = !isStockfishMove;
            }

            StockFishGameResult StockFishGameResult = new StockFishGameResult(Depth, StDepth, Strategy, Color,Elo, Move);

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

        private void AddMove(FullMoves fullMoves, MoveBase move, TimeSpan elapsed)
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
