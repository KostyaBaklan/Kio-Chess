using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using Engine.Models.Enums;
using Engine.DataStructures.Hash;
using Engine.Strategies.Models.Contexts;
using Engine.Models.Boards;
using Engine.Interfaces.Config;

namespace Engine.Strategies.Base.Null;

public abstract class NullStrategyBase : StrategyBase
{
    protected bool CanUseNull;
    protected bool IsNull;

    protected NullStrategyBase(int depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
        CanUseNull = false;
        var configuration = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
            .AlgorithmConfiguration.NullConfiguration;

        MinReduction = configuration.MinReduction;
        MaxReduction = (sbyte)configuration.MaxReduction;
        NullWindow = configuration.NullWindow;
        NullDepthOffset = configuration.NullDepthOffset;
        NullDepthReduction = configuration.NullDepthReduction;
    }

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pvMove = null)
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

        SetExtensionThresholds(depth, sortContext.Ply);

        if (CheckEndGame(moves.Count, result)) return result;

        if (moves.Count > 1)
        {
            SetResult(alpha, beta, depth, result, moves);
        }
        else
        {
            result.Move = moves[0];
        }

        return result;
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

        if (CheckDraw()) return 0;

        if (CanDoNullMove(depth))
        {
            MakeNullMove();
            int v = -NullSearch(-beta, (sbyte)(depth - NullDepthReduction - 1));
            UndoNullMove();
            if (v >= beta)
            {
                return v;
            }
        }

        SearchContext context = GetCurrentContext(alpha, beta, ref depth, pv);

        if(SetSearchValue(alpha, beta, depth, context))return context.Value;

        if (IsNull || isInTable && !shouldUpdate) return context.Value;

        return StoreValue(depth, (short)context.Value, context.BestMove.Key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SearchInternal(int alpha, int beta, sbyte depth, SearchContext context)
    {
        if (IsNull)
        {
            base.SearchInternal(alpha, beta, depth, context);
        }
        else
        {
            MoveBase move;
            int r;
            sbyte d = (sbyte)(depth - 1);
            int b = -beta;

            bool canUseNull = CanUseNull;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];

                Position.Make(move);

                CanUseNull = i > 0;

                r = -Search(b, -alpha, d);

                CanUseNull = canUseNull;

                Position.UnMake();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move;

                if (r >= beta)
                {
                    if (!move.IsAttack)
                    {
                        context.Add(move.Key);

                        move.History += 1 << depth;
                    }
                    break;
                }
                if (r > alpha)
                    alpha = r;
                if (!move.IsAttack) move.Butterfly++;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int NullSearch(int alpha, sbyte depth)
    {
        int beta = alpha + NullWindow;
        if (depth < 1) return Evaluate(alpha, beta);

        SearchContext context = GetCurrentContext(alpha, beta, ref depth);

        if (SetSearchValue(alpha, beta, depth, context)) return context.Value;

        return context.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SetResult(int alpha, int beta, sbyte depth, Result result, MoveList moves)
    {
        for (byte i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            Position.Make(move);

            CanUseNull = i > 0;

            int value = -Search(-beta, -alpha, (sbyte)(depth - 1));

            Position.UnMake();

            if (value > result.Value)
            {
                result.Value = value;
                result.Move = move;
            }

            if (value > alpha)
            {
                alpha = value;
            }

            if (alpha < beta) continue;
            break;
        }
    }
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //protected bool IsValidWindow(int alpha, int beta)
    //{
    //    return beta < SearchValue && beta - alpha > NullWindow;
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void UndoNullMove()
    {
        SwitchNull();
        Position.SwapTurn();
        IsNull = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void MakeNullMove()
    {
        SwitchNull();
        Position.SwapTurn();
        IsNull = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SwitchNull() => CanUseNull = !CanUseNull;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool CanDoNullMove(int depth) => CanUseNull && !MoveHistory.IsLastMoveWasCheck() && depth - 1 < Depth;
}
