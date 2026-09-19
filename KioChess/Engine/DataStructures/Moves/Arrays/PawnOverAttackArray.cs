using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Arrays
{
    public class PawnOverAttackArray
    {
        private const byte _mask = 7;
        private readonly AttackList[] _attacks;

        public PawnOverAttackArray(IEnumerable<PawnOverAttack> moveBases)
        {
            _attacks = new AttackList[8];
            var map = moveBases.GroupBy(m => m.From).ToDictionary(k => k.Key,
                g => g.ToList());

            foreach (var item in map)
            {
                var list = new AttackList(item.Value.Count);
                foreach (var m in item.Value)
                {
                    list.Add(m);
                }
                _attacks[item.Key & _mask] = list;
            }
        }

        public AttackList this[byte to]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _attacks[to & _mask];
        }
    }
}
