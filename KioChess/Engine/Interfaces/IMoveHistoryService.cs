using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IMoveHistoryService
    {
        int GetPly();
        MoveBase GetLastMove();
        void Add(MoveBase move);
        MoveBase Remove();
        bool CanDoBlackCastle();
        bool CanDoWhiteCastle();
        bool CanDoWhiteSmallCastle();
        bool CanDoWhiteBigCastle();
        bool CanDoBlackSmallCastle();
        bool CanDoBlackBigCastle();
        IEnumerable<MoveBase> GetHistory();
        bool IsThreefoldRepetition(ulong board);
        bool IsFiftyMoves();
        void Add(ulong board);
        void Remove(ulong board);
        bool IsLastMoveWasCheck();
        bool IsLastMoveWasPassed();
        bool IsLastMoveNotReducible();
        bool IsLast(short key);
        void AddFirst(MoveBase move);
        void SetCounterMove(short move);
        short GetCounterMove();
        void SetCounterMoves(int size);
    }
}
