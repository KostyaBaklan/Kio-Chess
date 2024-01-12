using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Models.Enums;
using Engine.DataStructures.Hash;
using Engine.Strategies.Models.Contexts;

namespace Engine.Strategies.Base.Null;

public abstract class NullExtendedStrategyBase : NullMemoryStrategyBase
{
    protected NullExtendedStrategyBase(int depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    public override int Search(int alpha, int beta, sbyte depth)
    {
        if (depth < 1) return Evaluate(alpha, beta);

        if (Position.GetPhase() == Phase.End)
            return EndGameStrategy.Search(alpha, beta, depth);

        MoveBase pv = null;
        bool shouldUpdate = false;
        bool isInTable = false;

        if (Table.TryGet(Position.GetKey(), out var entry))
        {
            isInTable = true;
            pv = GetPv(entry.PvMove);

            if (pv == null || entry.Depth < depth)
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
        }

        if (CheckDraw())  return 0; 

        if (CanDoNullMove(depth))
        {
            int reduction = depth > 6 ? MaxReduction : MinReduction;

            MakeNullMove();
            var v = -NullSearch((short)-beta, (sbyte)(depth - reduction - 1));
            UndoNullMove();
            if (v >= beta)
            {
                depth -= MaxReduction;
                if (depth < 1) return Evaluate(alpha, beta);
            }
        }

        SearchContext context = GetCurrentContext(alpha, beta, depth, pv);

        if(SetSearchValue(alpha, beta, depth, context))return context.Value;

        if (IsNull) return context.Value;

        if (isInTable && !shouldUpdate) return context.Value;

        return StoreValue(depth, (short)context.Value, context.BestMove.Key);
    }
}
