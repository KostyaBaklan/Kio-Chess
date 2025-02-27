using System.Diagnostics;

namespace StockFishCore.Execution
{
    public interface IExecutable
    {
        void Execute();

        void Log(int index, Stopwatch timer, double percentage);
    }
}
