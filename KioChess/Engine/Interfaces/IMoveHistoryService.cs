using DataAccess.Models;
using Engine.Dal.Models;
using Engine.DataStructures;
using Engine.Models.Moves;

namespace Engine.Interfaces;

public interface IMoveHistoryService
{
    short GetPly(); 
    void GetSequence(ref MoveKeyList keys);

    string GetSequenceKey();
    byte[] GetSequence();

    IPopularMoves GetBook();
    bool Any();
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
    List<MoveBase> GetOpeningMoves(IMoveProvider moveProvider);
    void SetOpening(List<BookMove> open);
    void CreateSequenceCache(Dictionary<string, IPopularMoves> map);
}
