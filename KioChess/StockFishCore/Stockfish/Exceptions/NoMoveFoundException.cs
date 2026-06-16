namespace StockFishCore.Stockfish.Exceptions
{
    public class NoMoveFoundException : Exception
    {
        public NoMoveFoundException() : base("No moves found or parsed after SF search") { }
    }
}
