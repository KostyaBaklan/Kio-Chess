using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Models;

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
        IEnumerable<MoveBase> GetAllMoves(Square cell, Piece piece);
        int GetPieceValue(Square square);
        IBoard GetBoard();
        IEnumerable<MoveBase> GetHistory();
        Phase GetPhase();
        bool CanWhitePromote();
        bool CanBlackPromote();
        void SaveHistory();
        bool IsDraw();
        void MakeFirst(MoveBase move);
        MoveList GetAllMoves(SortContext sortContext);
        MoveList GetAllAttacks(SortContext sortContext);
        void GetWhitePromotionAttacks(AttackList attacks);
        void GetWhiteAttacks(AttackList attacks);
        void GetBlackPromotionAttacks(AttackList attacks);
        void GetBlackAttacks(AttackList attacks);
    }
}
