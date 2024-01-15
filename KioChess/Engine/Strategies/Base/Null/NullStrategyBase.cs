﻿using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using Engine.Models.Enums;
using Engine.Strategies.Models.Contexts;

namespace Engine.Strategies.Base.Null;

public abstract class NullStrategyBase : StrategyBase
{
    protected bool CanUseNull;
    protected bool IsNull;
    protected int MinReduction;
    protected sbyte MaxReduction;
    protected int NullWindow;
    protected int NullDepthReduction;
    protected int NullDepthOffset;

    public NullStrategyBase(int depth, IPosition position) : base(depth, position)
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

    public override IResult GetResult(int alpha, int beta, sbyte depth, MoveBase pv = null)
    {
        CanUseNull = false;
        Result result = new Result();

        if (IsDraw(result))
        {
            return result;
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[Depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        DistanceFromRoot = sortContext.Ply; MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Search(int alpha, int beta, sbyte depth)
    {
        if (depth < 1) return Evaluate(alpha, beta);

        if (Position.GetPhase() == Phase.End)
            return EndGameStrategy.Search(alpha, beta, depth);

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

        SearchContext context = GetCurrentContext(alpha, beta, depth);

        if(SetSearchValue(alpha, beta, depth, context))return context.Value;

        return context.Value;
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

        SearchContext context = GetCurrentContext(alpha, beta, depth);

        if(SetSearchValue(alpha, beta, depth, context))return context.Value;

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
