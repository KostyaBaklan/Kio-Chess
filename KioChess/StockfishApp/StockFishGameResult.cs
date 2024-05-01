using Engine.Models.Moves;
using Engine.Strategies.Base;
using StockfishApp.Models;
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
    public StockFishGameResult(short depth, short stDepth, StrategyBase strategy, bool isStockfishMove)
    {
        Depth = depth;
        StockFishDepth = stDepth;
        Strategy = strategy.ToString();
        IsStockfishMove = isStockfishMove;
    }

    public StockFishGameResultType Output { get; set; }
    public List<MoveBase> History { get; set; }
    public FullMoves Moves { get;  set; }
    public int Value { get;  set; }
    public int Static { get; set; }
    public string Board { get; set; }
    public short Depth { get; }
    public short StockFishDepth { get; }
    public string Strategy { get; }
    public bool IsStockfishMove { get; }
    public TimeSpan Time { get; set; }

    public override string ToString()
    {
        StringBuilder stringBuilder= new StringBuilder();

        stringBuilder.AppendLine($"Depth = {Depth}, StockFish Depth = {StockFishDepth}, Strategy = {Strategy}, Is Stockfish Move = {IsStockfishMove}");
        stringBuilder.AppendLine($"Output = {Output}, Value = {Value}, Static = {Static}, Time = {Time}");
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
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Depth = {Depth}, StockFish Depth = {StockFishDepth}, Strategy = {Strategy}, Is Stockfish Move = {IsStockfishMove}");
        stringBuilder.AppendLine($"Output = {Output}, Value = {Value}, Static = {Static}, Time = {Time}");

        return stringBuilder.ToString();
    }
}

enum StockFishGameResultType
{
    White,
    Black,
    Draw
}