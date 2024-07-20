
namespace StockfishApp.Exceptions
{
    public class NoMoveFoundException:Exception
    {
        public NoMoveFoundException() : base("No moves found or parsed afetr SF search") { }
    }
}
