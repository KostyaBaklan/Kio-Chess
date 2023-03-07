using Common;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Strategies.AB;
using Engine.Strategies.Aspiration;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Strategies.Null;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Tools.Common;

namespace Tests
{
    internal class PerformanceTest
    {
        private static readonly TestModel _model = new TestModel();

        public static void Test(string[] args)
        {
            if (!Directory.Exists("Log"))
            {
                Directory.CreateDirectory("Log");
            }

            var depth = short.Parse(args[1]);

            var iterations = int.Parse(args[2]);
            var game = args[3];
            //bool shouldPrintPosition = args.Length<=4 || bool.Parse(args[4]);

            _model.Depth = depth;
            _model.Game = game;

            IPosition position = new Position();

            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            var moves = GameProvider.GetMoves(game);

            position.MakeFirst(moves[0]);
            for (int i = 1; i < moves.Count; i++)
            {
                MoveBase moveBase = moves[i];
                position.Make(moveBase);
            }

            Dictionary<string, Func<short, IPosition, StrategyBase>> strategyFactories =
                new Dictionary<string, Func<short, IPosition, StrategyBase>>
                {
                    {"lmr", (d, p) => new LmrStrategy(d, p)},
                    {"lmrd", (d, p) => new LmrDeepStrategy(d, p)},
                    {"lmr_null", (d, p) => new NullLmrStrategy(d, p)},
                    {"lmrd_null", (d, p) => new NullLmrDeepStrategy(d, p)},

                    {"ab", (d, p) => new NegaMaxMemoryStrategy(d, p)},
                    {"ab_null", (d, p) => new NullNegaMaxMemoryStrategy(d, p)},
                    {"null_ext", (d, p) => new NullExtendedStrategy(d, p)},

                    {"lmr_asp", (d, p) => new LmrAspirationStrategy(d, p)},
                    {"lmrd_asp", (d, p) => new LmrDeepAspirationStrategy(d, p)}
                };

            StrategyBase strategy = strategyFactories[args[0]](depth, position);
            _model.Strategy = strategy.ToString();

            var file = Path.Combine("Log", $"{strategy}_D{depth}_{game}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log");

            Play(iterations, strategy, position, moveProvider, game);

            _model.Calculate();
            _model.Position = position.ToString();

            var content = JsonConvert.SerializeObject(_model, Formatting.Indented);
            File.WriteAllText(file,content, Encoding.BigEndianUnicode);

            //position.GetBoard().PrintCache(Path.Combine("Log", $"See_Cache_{strategy}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log"));
        }

        private static void Play(int depth, StrategyBase strategy, IPosition position, IMoveProvider moveProvider, string game)
        {
            bool isWaiting = false;

            ResultDto dto = null;

            var formatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();

            TimeSpan total = TimeSpan.Zero;

            Process testTool = new Process
            {
                EnableRaisingEvents= true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = "TestStrategyTool.exe",
                    Arguments = $"{_model.Depth - 1} {game}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError= true,
                    RedirectStandardInput = true
                }
            };

            testTool.OutputDataReceived += (s, a) =>
            {
                if (!string.IsNullOrWhiteSpace(a.Data))
                {
                    dto = JsonConvert.DeserializeObject<ResultDto>(a.Data);
                    isWaiting = false; 
                }
            }; 
            
            testTool.ErrorDataReceived += (s, a) =>
            {
                dto = JsonConvert.DeserializeObject<ResultDto>(a.Data);
                isWaiting = false;
            };

            testTool.Start();

            testTool.BeginOutputReadLine();

            for (int i = 0; i < depth; i++)
            {
                while (strategy.IsBlocked())
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
                }

                var timer = new Stopwatch();
                timer.Start();

                var result = strategy.GetResult();

                timer.Stop();

                strategy.ExecuteAsyncAction();

                var move = result.Move;
                if (move != null)
                {
                    position.Make(move);

                    //Debugger.Launch();

                    isWaiting = true;

                    testTool.StandardInput.WriteLine(move.Key);
                }
                else
                {
                    var pizdetsZdesNull = "Pizdets zdes NULL !!!";
                    Console.WriteLine(pizdetsZdesNull);
                    Console.WriteLine($"Game Result = {result.GameResult} !!!");
                    Console.WriteLine(position);
                    break;
                }

                MoveModel moveModel = new MoveModel();
                var timerElapsed = timer.Elapsed;
                total += timerElapsed;
                var logMessage = $"{i + 1} - Elapsed {timerElapsed}, Total = {total}";

                moveModel.Number = i + 1;
                moveModel.Time = timerElapsed;

                var currentProcess = Process.GetCurrentProcess();
                var memory = currentProcess.WorkingSet64;

                Console.WriteLine($"{logMessage} Table = {strategy.Size},  Memory = {memory/1024} KB");

                moveModel.Table = strategy.Size;
                moveModel.Memory = memory/1024;
                moveModel.White = formatter.Format(move);

                while (isWaiting)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }

                var r = dto;
                var m = r.Move;
                if (m < 0)
                {
                    Console.WriteLine($"{i + 1} The opponent has no moves, Game Result = {r.GameResult} !!!");
                }
                else
                {
                    move = moveProvider.Get(m);
                    position.Make(move);
                    moveModel.Black = formatter.Format(move);
                }

                moveModel.StaticValue = position.GetValue();
                moveModel.Material = position.GetStaticValue();

                _model.Moves.Add(moveModel);

                if (m < 0) break;
            }

            testTool.StandardInput.WriteLine("end");
        }
    }
}