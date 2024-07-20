using System.Diagnostics;

namespace StockFishTool
{
    public interface IExecutable
    {
        void Execute();

        void Log(int index, Stopwatch timer, double percentage);
    }
}
