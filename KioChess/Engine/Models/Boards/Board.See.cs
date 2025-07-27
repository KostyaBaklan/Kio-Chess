using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchange(AttackBase attack)
        {
            _attackEvaluationService.Initialize(_boards);
            return _attackEvaluationService.StaticExchange(attack);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchangeWithPins(AttackBase attack)
        {
            _attackEvaluationService.Initialize(_boards);
            return _attackEvaluationService.StaticExchangeWithPins(attack);
        }
    }
}
