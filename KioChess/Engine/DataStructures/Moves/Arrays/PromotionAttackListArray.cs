using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Arrays
{
    public class PromotionAttackListArray
    {
        private const byte _mask = 7;
        private readonly PromotionAttackArray[] _promotions;

        public PromotionAttackListArray(IEnumerable<PromotionAttack> promotions)
        {
            _promotions = new PromotionAttackArray[8];
            var map = promotions.GroupBy(p => p.From).ToDictionary(k => k.Key,
                g => g.ToList());

            foreach (var item in map)
            {
                _promotions[item.Key & _mask] = new PromotionAttackArray(item.Value);
            }
        }

        public PromotionAttackArray this[byte to]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _promotions[to & _mask];
        }

    }
}