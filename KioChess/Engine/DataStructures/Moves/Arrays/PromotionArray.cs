using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards.Buffers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Arrays
{
    public class PromotionArray
    {
        private const byte _mask = 7;
        private readonly PromotionList[] _promotions;
        private readonly RankBuffer<byte> _indicies;

        public PromotionArray(List<PromotionMove> promotions)
        {
            _indicies = new RankBuffer<byte>();

            var map = promotions.GroupBy(p => p.To)
                .ToDictionary(k => k.Key,
                g => g.ToList());

            _promotions = new PromotionList[map.Count];

            byte index = 0;
            foreach (var item in map)
            {
                _indicies[item.Key & _mask] = index;

                PromotionList list = new PromotionList(4);
                foreach (var pr in item.Value)
                {
                    list.Add(pr);
                }
                _promotions[index] = list;

                index++;
            }
        }

        public PromotionList this[byte to]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _promotions[_indicies[to & _mask]];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromotionList[] GetAll() => _promotions;
    }
}