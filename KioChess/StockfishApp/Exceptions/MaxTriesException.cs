namespace StockfishApp.Exceptions
{
    public class MaxTriesException : Exception
    {
        public MaxTriesException(int maxTries, string method, string lastLine)
            : base($"Max tries {maxTries} was reached inside {method}. Last line {lastLine}") { }
    }
}