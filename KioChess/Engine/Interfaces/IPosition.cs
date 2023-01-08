using Engine.DataStructures.Moves;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces
{
    public interface IPosition
    {
        ulong GetKey();
        short GetValue();
        int GetStaticValue();
        int GetKingSafetyValue();
        int GetPawnValue();
        int GetOpponentMaxValue();
        Turn GetTurn();
        bool GetPiece(Square cell, out Piece? piece);

        void Make(MoveBase move);
        void UnMake();
        void Do(MoveBase move);
        void UnDo(MoveBase move);
        void SwapTurn();

        IEnumerable<AttackBase> GetAllAttacks(Square cell, Piece piece);
        IEnumerable<MoveBase> GetAllMoves(Square cell, Piece piece);
        MoveBase[] GetAllAttacks(IMoveSorter sorter);
        AttackList GetWhiteAttacks();
        AttackList GetBlackAttacks();
        MoveBase[] GetAllMoves(IMoveSorter sorter, MoveBase pvMove = null);
        int GetPieceValue(Square square);
        IBoard GetBoard();
        IEnumerable<MoveBase> GetHistory();
        Phase GetPhase();
        bool CanWhitePromote();
        bool CanBlackPromote();
        void SaveHistory();
        bool IsDraw();
        bool AnyMoves();
    }
}
