using Engine.Models.Boards;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IAttackEvaluationService
    {
        void Initialize(BitBoard[] boards);
        short StaticExchange(AttackBase attack);
        void SetBoard(IBoard board);
    }
}
