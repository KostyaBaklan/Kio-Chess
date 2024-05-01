using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using StockfishApp.Core;
using StockfishApp.Models;
using System.Diagnostics;

namespace StockfishApp
{
    internal class StockFishGame
    {
        public StockFishGame(short depth, short stDepth, string game, bool isStockfishMove)
        {
            Depth = depth;
            StDepth = stDepth; 
            
            Stockfish = new Stockfish(@"..\..\..\..\stockfish\stockfish-windows-x86-64-avx2.exe", stDepth);

            Position = new Position();
            var service = Boot.GetService<ITranspositionTableService>();

            var table = service.Create(depth);

            IStrategyFactory strategyFactory = Boot.GetService<IStrategyFactory>();

            Strategy = strategyFactory.GetStrategy(depth, Position, table, game);

            IsStockfishMove = isStockfishMove;

            Count = 1;
        }

        public int Count { get; set; }
        public short Depth { get; }
        public short StDepth { get; }
        public Stockfish Stockfish { get; set; }
        public Position Position { get; set; }
        public StrategyBase Strategy { get; set; }
        public bool IsStockfishMove { get; set; }

        internal StockFishGameResult Play()
        {
            var timer = Stopwatch.StartNew();
            var isStockfishMove = IsStockfishMove;
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
                            result = Strategy.GetResult(short.MinValue, short.MaxValue, 1);
                        }
                        else
                        {
                            Debugger.Launch();
                        }
                    }
                    else
                    {
                        AddMove(fullMoves, move);
                    }
                }
                else
                {
                    result = Strategy.GetResult();

                    if (result.Move != null)
                    {

                        AddMove(fullMoves, result.Move);
                    }
                }

                isStockfishMove = !isStockfishMove;
            }

            StockFishGameResult StockFishGameResult = new StockFishGameResult(Depth, StDepth, Strategy, IsStockfishMove);

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

        private void AddMove(FullMoves fullMoves, MoveBase move)
        {
            fullMoves.Add(move);
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
                Console.WriteLine($"{move}. V = {Position.GetValue()}, S = {Position.GetStaticValue()}"); 
                Count++;
            }
            else
            {
                Console.Write($"{Count} {move} ");
            }
        }
    }
}
