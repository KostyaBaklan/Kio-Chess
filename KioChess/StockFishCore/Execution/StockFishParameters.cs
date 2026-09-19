using Engine.Services;
using System.Diagnostics;

namespace StockFishCore.Execution
{
    public class StockFishParameters : IComparable<StockFishParameters>, IExecutable
    {
        private static string Exe;
        public int Elo { get; set; }
        public int Depth { get; set; }
        public int StockFishDepth { get; set; }
        public string Color { get;  set; }
        public string Strategy { get;  set; }
        public string Move { get;  set; }
        public int RunTimeId { get; set; }

        public static void Initialize() => Exe = @"StockfishApp.exe";

        public void Execute()
        {
            using (Process process = Process.Start(Exe, $"{Depth} {StockFishDepth} {Strategy} {Color} {Elo} {Move} {RunTimeId}"))
            {
                if (process == null)
                    return;

                process.WaitForExit();
                Delay();
            }
        }

        private void Delay()
        {
            switch (Depth)
            {
                case 5: Thread.Sleep(100); break;
                case 6: Thread.Sleep(500); break;
                case 7: Thread.Sleep(1000); break;
                case 8:
                    Thread.Sleep(3000); break;
                case 9:
                    Thread.Sleep(5000); break;
                default:
                    Thread.Sleep(7000); break;
            }
        }

        public void Log(int i, Stopwatch timer, double v)
        {
            var mp = Boot.GetService<MoveProvider>();
            var moves = Move.Split('-').Select(x => mp.Get(short.Parse(x)).ToLightString());
            string message = $"I = {i}, T = {timer.Elapsed}, P = {v}%, D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C = {Color}, L={Elo}, M={string.Join("-", moves)}";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        public override string ToString()
        {
            var mp = Boot.GetService<MoveProvider>();
            var moves = Move.Split('-').Select(x => mp.Get(short.Parse(x)).ToLightString());
            return $"D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C = {Color}, L={Elo}, M={string.Join("-", moves)}";
        }

        public int CompareTo(StockFishParameters other)
        {
            var compare = Depth.CompareTo(other.Depth);

            if (compare == 0)
            {
                compare = StockFishDepth.CompareTo(other.StockFishDepth);
                if (compare == 0)
                {
                    return Elo.CompareTo(other.Elo);
                }
            }

            return compare;
        }
    }
}