using Engine.Models.Boards;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IAttackEvaluationService
    {
        void Initialize(BitBoard[] boards);
        int StaticExchange(AttackBase attack);
        void SetBoard(IBoard board);
    }
}
