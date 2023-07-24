using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Strategies.Models;

namespace Engine.Strategies.End
{
    public class LmrDeepEndGameAbStrategy : LmrDeepEndGameStrategy
    {
        public LmrDeepEndGameAbStrategy(short depth, IPosition position, TranspositionTable table = null) 
            : base(depth, position, table)
        {
        }

        public override IResult GetResult()
        {
            //if(Position.GetPhase()!=Phase.End)
            //    return GetResult((short)-SearchValue, SearchValue, (sbyte)(Depth - 1));
            if (IsLateEndGame())
                return GetResult((short)-SearchValue, SearchValue, (sbyte)(Depth + 1));
            return GetResult((short)-SearchValue, SearchValue, Depth);
        }

        public override short Search(short alpha, short beta, sbyte depth)
        {
            if (depth < 1) return Evaluate(alpha, beta);

            if (CheckEndGameDraw()) 
            {
                return (short)-Position.GetValue();
            }

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;

            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                isInTable = true;

                if (entry.Depth < depth)
                {
                    shouldUpdate = true;
                }
                else
                {
                    if (entry.Value >= beta)
                        return entry.Value;

                    if (entry.Value > alpha)
                        alpha = entry.Value;
                }

                pv = GetPv(entry.PvMove);
            }

            SearchContext context = GetCurrentContext(alpha, beta, depth, pv);

            if (SetSearchValue(alpha, beta, depth, context)) return context.Value;

            if (isInTable && !shouldUpdate) return context.Value;

            return StoreValue(depth, context.Value, context.BestMove.Key);
        }
    }
}
