using Engine.Models.Helpers;
using System.Diagnostics;

namespace StockFishCore.Execution
{
    public class BranchExecutor
    {
        private BranchItem _branch;

        public BranchExecutor(BranchItem item)
        {
            _branch = item;
        }

        public int Execute()
        {
            var t = Stopwatch.StartNew();

            int threads = 13 * Environment.ProcessorCount / 20;

            List<StockFishParameters> stockFishParameters = CreateStockFishParameters(threads);

            ParallelExecutor parallelExecutor = new ParallelExecutor(threads, stockFishParameters);

            parallelExecutor.Execute();

            t.Stop();

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
            Console.WriteLine($"Time = {t.Elapsed}, Total = {stockFishParameters.Count}, Average = {TimeSpan.FromMilliseconds(t.ElapsedMilliseconds / stockFishParameters.Count)}");


            Console.WriteLine();
            Console.WriteLine(" ----- ");
            Console.WriteLine();

            return stockFishParameters.Count;
        }

        private List<StockFishParameters> CreateStockFishParameters(int threads)
        {
            StockFishParameters.Initialize();
            List<StockFishParameters> stockFishParameters = new List<StockFishParameters>();
            string[] strategies = new string[] { "lmrd", "id", "asp" };
            string[] colors = { "w", "b" };
            string[] moves = new string[] { "7686-11778", "7686-11436", "7686-11434", "7686-11443", "7686-11439", "7688-11434", "7688-11438", "7688-11435", "7688-11439", "7688-11436", "7688-11443", "7688-11778", "7750-11778", "7750-11443", "7684-11778", "7684-11438", "7693-11436", "7683-11778", "7683-11436", "7732-11436" };

            //string[] moves = new string[] { "7686-11778", "7686-11436", "7686-11434", "7686-11443", "7686-11439", "7688-11434"};

            var depthSkillMap = new Dictionary<int, List<int>>
            {
                {5, new List<int> { 2500}},
                {6, new List<int> { 2500}},
                {7, new List<int>{ 2500}},
                {8, new List<int> { 2500 }},
                {9, new List<int>{ 2500}},
                {10, new List<int>{2500}},
                //{11, new List<int>{2500}}
            };

            foreach (KeyValuePair<int, List<int>> dsm in depthSkillMap)
            {
                foreach (int skillMap in dsm.Value)
                {
                    for (int c = 0; c < colors.Length; c++)
                    {
                        for (int s = 0; s < strategies.Length; s++)
                        {
                            for (int m = 0; m < moves.Length; m++)
                            {
                                StockFishParameters parameters = new()
                                {
                                    Elo = skillMap,
                                    Depth = dsm.Key,
                                    StockFishDepth = dsm.Key,
                                    Color = colors[c],
                                    Strategy = strategies[s],
                                    Move = moves[m],
                                    RunTimeId = _branch.Id
                                };

                                stockFishParameters.Add(parameters);
                            }
                        }
                    }
                }
            }

            stockFishParameters.Shuffle();

            stockFishParameters.Sort();

            List<List<StockFishParameters>> parametersSet = new List<List<StockFishParameters>>();

            for (int i = 0; i < threads; i++)
            {
                parametersSet.Add(new List<StockFishParameters>());
            }

            for (int i = 0; i < stockFishParameters.Count; i++)
            {
                parametersSet[i % threads].Add(stockFishParameters[i]);
            }

            stockFishParameters.Clear();

            foreach (var set in parametersSet)
            {
                set.Shuffle();
                stockFishParameters.AddRange(set);
            }

            return stockFishParameters;
        }
    }
}
