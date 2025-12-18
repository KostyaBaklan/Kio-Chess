using System.Diagnostics;

namespace StockFishCore.Execution
{
    public interface IExecutable
    {
        int Depth { get; }
        void Execute();

        void Log(int index, Stopwatch timer, double percentage);
    }
}
