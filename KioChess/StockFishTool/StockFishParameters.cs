﻿using StockFishTool;
using System.Diagnostics;

public class StockFishParameters:IComparable<StockFishParameters>, IExecutable
{
    private static string Exe;
    public int Elo { get; internal set; }
    public int Depth { get; internal set; }
    public int StockFishDepth { get; internal set; }
    public string Color { get; internal set; }
    public string Strategy { get; internal set; }

    internal static void Initialize()
    {
        Exe = @"StockfishApp.exe";
    }

    public void Execute()
    {
        Process process = Process.Start(Exe, $"{Depth} {StockFishDepth} {Strategy} {Color} {Elo}");

        process.WaitForExit();
    }

    public void Log(int i, Stopwatch timer, double v)
    {
        string message = $"I = {i}, T = {timer.Elapsed}, P = {v}%, D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C = {Color}, L={Elo}";

        Console.WriteLine(message);
    }

    public override string ToString()
    {
        return $"D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C = {Color}, L={Elo}";
    }

    public int CompareTo(StockFishParameters other)
    {
        var compare = Depth.CompareTo(other.Depth);

        if (compare == 0)
        {
            compare = StockFishDepth.CompareTo(other.StockFishDepth);
            if(compare == 0)
            {
                return Elo.CompareTo(other.Elo);
            }
        }

        return compare;
    }
}