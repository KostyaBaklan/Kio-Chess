using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using StockfishApp.Models;
using StockFishCore;
using System.Text;

internal class StockFishGameResult
{
    public StockFishGameResult(short depth, short stDepth, StrategyBase strategy, string color, int elo)
    {
        Depth = depth;
        StockFishDepth = stDepth;
        Strategy = strategy.Type;
        Color = color;
        Elo = elo;
    }

    public StockFishGameResultType Output { get; set; }
    public List<MoveBase> History { get; set; }
    public FullMoves Moves { get;  set; }
    public int Value { get;  set; }
    public int Elo { get; set; }
    public int Static { get; set; }
    public string Board { get; set; }
    public short Depth { get; }
    public short StockFishDepth { get; }
    public StrategyType Strategy { get; }
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

    internal string ToShort()
    {
        return $"E = {Time}, D = {Depth}, SD = {StockFishDepth}, S = {Strategy}, C = {Color} O = {Output}, V = {Value}, S = {Static}, L = {Elo}";
    }
}

