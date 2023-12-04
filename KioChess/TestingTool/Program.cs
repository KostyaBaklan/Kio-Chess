using TestingTool;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        PerformanceTest.Test(args);

        //PerformanceTest.Test(new[] { "lmr", "8", "10", "king"});
        //PerformanceTest.Test(new[] { "lmrd_bs_hc", "7", "6", false.ToString() });
        //PerformanceTest.Test(new[] { "lmr_es_hc", "7", "6", false.ToString() });
        //PerformanceTest.Test(new[] { "lmrd_es_hc", "8", "6", "king" });
        //PerformanceTest.Test(new[] { "lmr_as_hc", "7", "6", false.ToString() });
        //PerformanceTest.Test(new[] { "lmrd_as_hc", "7", "6", false.ToString() });

        //OpenningsTest.Opennings();

        Console.WriteLine("Finished");

        //Console.ReadLine();
    }
}