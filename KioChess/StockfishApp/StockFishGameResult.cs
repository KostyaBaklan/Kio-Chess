using Engine.DataStructures;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using StockfishApp.Models;
using System.IO;
using System.Text;


internal class StockFishGameResultJson
{
    public string Output { get; set; }
    public List<short> History { get; set; }
    public string Moves { get; set; }
    public int Value { get; set; }
    public int Static { get; set; }
    public string Board { get; set; }
    public TimeSpan Time { get; set; }
}

internal class StockFishGameResult
{
    public StockFishGameResult(short depth, short stDepth, StrategyBase strategy, string color, int skill)
    {
        Depth = depth;
        StockFishDepth = stDepth;
        Strategy = strategy.ToString();
        Color = color;
        Skill = skill;
    }

    public StockFishGameResultType Output { get; set; }
    public List<MoveBase> History { get; set; }
    public FullMoves Moves { get;  set; }
    public int Value { get;  set; }
    public int Skill { get; set; }
    public int Static { get; set; }
    public string Board { get; set; }
    public short Depth { get; }
    public short StockFishDepth { get; }
    public string Strategy { get; }
    public string Color { get; }
    public TimeSpan Time { get; set; }

    public override string ToString()
    {
        StringBuilder stringBuilder= new StringBuilder();

        stringBuilder.AppendLine(ToShort());
        stringBuilder.AppendLine(string.Join(' ', History.Select(x=>x.Key)));
        stringBuilder.AppendLine(string.Join(' ', History));
        stringBuilder.AppendLine(Moves.ToString());
        stringBuilder.AppendLine(Board);

        return stringBuilder.ToString();
    }

    public StockFishGameResultJson ToJson()
    {
        return new StockFishGameResultJson
        {
            Output = Output.ToString(),
            History = History.Select(x => x.Key).ToList(),
            Moves = Moves.ToString(),
            Value = Value,
            Static = Static,
            Board = Board,
            Time= Time
        };
    }

    internal string ToShort()
    {
        return $"D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C= {Color} O = {Output}, V = {Value}, S = {Static}, T = {Time}, L = {Skill}";
    }
    internal void Save(string fullName)
    {
        var dir = Path.Combine(fullName,$"{Depth}_{StockFishDepth}_{Strategy}_{Color}_{Skill}");
        DirectoryInfo directoryInfo;

        if (!Directory.Exists(dir))
        {
            directoryInfo = Directory.CreateDirectory(dir);
        }
        else
        {
            directoryInfo = new DirectoryInfo(dir);
        }

        File.WriteAllText($"{directoryInfo.FullName}\\{new Random().Next()}_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_ffff")}.txt", ToString());
    }
}

enum StockFishGameResultType
{
    White,
    Black,
    Draw
}