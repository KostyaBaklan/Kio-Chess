using System.Diagnostics;

internal class StockFishParameters
{
    private static string Exe;
    public int SkillLevel { get; internal set; }
    public int Depth { get; internal set; }
    public int StockFishDepth { get; internal set; }
    public string Color { get; internal set; }
    public string Strategy { get; internal set; }

    internal static void Initialize()
    {
        Exe = @"StockfishApp.exe";
    }

    internal void Execute()
    {
        Process process = Process.Start(Exe, $"{Depth} {StockFishDepth} {Strategy} {Color} {SkillLevel}");

        process.WaitForExit();
    }

    internal void Log(int i, Stopwatch timer, double v)
    {
        string message = $"I = {i}, T = {timer.Elapsed}, P = {v}%, D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C = {Color}, L={SkillLevel}";

        Console.WriteLine(message);
    }
}