using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.End;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Base;

public abstract class MemoryStrategyBase : StrategyBase
{
    protected sbyte AlphaDepth;
    protected readonly TranspositionTable Table;
    protected MemoryStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position)
    {
        if (table == null)
        {
            var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

            Table = service.Create(depth);
        }
        else
        {
            Table = table;
        }

        AlphaDepth = (sbyte)(depth - 2);
    }
    public override int Size => Table.Count;

    public override IResult GetResult(short alpha, short beta, sbyte depth, MoveBase pvMove = null)
    {
        Result result = new Result();
        if (IsDraw(result))
        {
            return result;
        }

        MoveBase pv = pvMove;
        if (pv == null)
        {
            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = GetPv(entry.PvMove);
            }
        }
        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[Depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        DistanceFromRoot = sortContext.Ply; 
        MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

        if (CheckEndGame(moves.Count, result)) return result;

        if (moves.Count > 1)
        {
            result.Move = pv;
            SetResult(alpha, beta, depth, result, moves);
        }
        else
        {
            result.Move = moves[0];
        }

        return result;
    }

    public override short Search(short alpha, short beta, sbyte depth)
    {
        if (CheckDraw())
            return 0;

        if (depth < 1) return Evaluate(alpha, beta);

        if (Position.GetPhase() == Phase.End)
        {
            if (depth < 6 && MaxExtensionPly > MoveHistory.GetPly())
            {
                depth++;
            }
            return EndGameStrategy.Search(alpha, beta, depth);
        }

        TranspositionContext transpositionContext = GetTranspositionContext(beta, depth);
        if (transpositionContext.IsBetaExceeded) return beta;

        SearchContext context = GetCurrentContext(alpha, beta, depth, transpositionContext.Pv);

        if(SetSearchValue(alpha, beta, depth, context))return context.Value;

        if (transpositionContext.NotShouldUpdate) return context.Value;

        return StoreValue(depth, context.Value, context.BestMove.Key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionContext GetTranspositionContext(short beta, sbyte depth)
    {
        TranspositionContext context = new TranspositionContext();

        if (!Table.TryGet(Position.GetKey(), out var entry)) return context;

        context.Pv = GetPv(entry.PvMove);

        if (context.Pv == null || entry.Depth < depth) return context;

        if (entry.Value >= beta)
            context.IsBetaExceeded = true;
        else
            context.NotShouldUpdate = true;

        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected short StoreValue(sbyte depth, short value, short bestMove)
    {
        Table.Set(Position.GetKey(), new TranspositionEntry { Depth = depth, Value = value, PvMove = bestMove });

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Table.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsBlocked()
    {
        return Table.IsBlocked();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ExecuteAsyncAction()
    {
        Table.Update();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected MoveBase GetPv(short entry)
    {
        var pv = MoveProvider.Get(entry);

        return pv.Turn != Position.GetTurn()
            ? null
            : pv;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsThesameColor(short entry)
    {
        return MoveProvider.Get(entry).Turn == Position.GetTurn();
    }
    protected override StrategyBase CreateEndGameStrategy()
    {
        short depth = (short)(Depth + 1);
        if (Depth < MaxEndGameDepth)
        {
            depth++;
        }
        return new IdLmrDeepEndStrategy(depth, Position, Table);
    }
}
