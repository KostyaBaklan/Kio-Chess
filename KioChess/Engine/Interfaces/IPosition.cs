using Engine.DataStructures.Moves.Lists;
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
        bool GetPiece(byte cell, out byte? piece);

        void Make(MoveBase move);
        void UnMake();
        void Do(MoveBase move);
        void UnDo(MoveBase move);
        void SwapTurn();
        IEnumerable<MoveBase> GetAllMoves(byte cell, byte piece);
        int GetPieceValue(byte square);
        IBoard GetBoard();
        IEnumerable<MoveBase> GetHistory();
        byte GetPhase();
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
        bool IsBlockedByBlack(byte position);
        bool IsBlockedByWhite(byte position);
        void GetWhiteAttacksTo(byte to, AttackList attackList);
        void GetBlackAttacksTo(byte to, AttackList attackList);
    }
}
