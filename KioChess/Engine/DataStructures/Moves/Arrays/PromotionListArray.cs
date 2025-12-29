using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Arrays
{
    public class PromotionListArray
    {
        private const byte _mask = 7;
        private readonly PromotionArray[] _promotions;

        public PromotionListArray(IEnumerable<PromotionMove> promotions)
        {
            _promotions = new PromotionArray[8];
            var map = promotions.GroupBy(p => p.From).ToDictionary(k => k.Key,
                g => g.ToList());

            foreach (var item in map)
            {
                _promotions[item.Key & _mask] = new PromotionArray(item.Value);
            }
        }

        public PromotionArray this[byte to]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _promotions[to & _mask];
        }
    }
}
