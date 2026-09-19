using StockFishCore.Stockfish.Models;

namespace StockFishCore.Stockfish
{
    public interface IStockfish
    {
        int Depth { get; set; }
        void SetPosition(params string[] move);
        void SetPosition(string fen, params string[] moves);
        string GetBoardVisual();
        string GetFenPosition();
        void SetFenPosition(string fenPosition);
        string GetBestMove();
        string GetBestMoveTime(int time = 1000);
        bool IsMoveCorrect(string moveValue);
        Evaluation GetEvaluation();
    }
}
