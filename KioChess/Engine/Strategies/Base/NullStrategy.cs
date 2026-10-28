using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Transposition;
using Engine.Interfaces.Config;
using Engine.Strategies.Models.Contexts;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Base;

/// <summary>
/// Zero-window search used only below a null move. Owns a small transposition table so null-subtree
/// entries never pollute the main table. Stores bounds only, never extends, never writes history/butterfly.
/// </summary>
public class NullStrategy : StrategyBase
{
    private int _nullRootPly;

    /// <summary>Benchmark hook: when greater than zero, overrides the null table size (MB per colour).</summary>
    public static int CapacityOverrideMB { get; set; }

    public NullStrategy(int depth, Position position)
        : base(depth, position, CreateTable(depth, position))
    {
        InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position));
    }

    public override StrategyType Type => StrategyType.NULL;

    protected override StrategyBase CreateEndGameStrategy() => null;

    protected override NullStrategy CreateNullStrategy() => null;

    private static TranspositionTable CreateTable(int depth, Position position)
    {
        int capacity = depth switch
        {
            > 11 => 64,
            11 or 10 => 32,
            9 => 16,
            8 => 8,
            _ => 4
        };

        if (CapacityOverrideMB > 0)
        {
            capacity = CapacityOverrideMB;
        }

        return new TranspositionTable(capacity, position.GetBoard(),
            ContainerLocator.Current.Resolve<IConfigurationProvider>(), "Null");
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ProbeWhite(int beta, int depth, int rootPly)
    {
        SetRoot(rootPly);
        return SearchWhiteNull(beta, depth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ProbeBlack(int beta, int depth, int rootPly)
    {
        SetRoot(rootPly);
        return SearchBlackNull(beta, depth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetRoot(int rootPly)
    {
        _nullRootPly = MoveHistory.GetPly();
        Ply = rootPly;
    }

    private int SearchWhiteNull(int beta, int depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateWhite(beta - NullWindow, beta);

        var entry = Table.GetWhite();
        short pv = -1;
        if (entry.Depth > 0)
        {
            if (entry.Depth >= depth)
            {
                int ttValue = FromTTValue(entry.Value);
                switch (entry.Type)
                {
                    case TranspositionEntryType.LowerBound:
                        if (ttValue >= beta)
                            return ttValue;
                        break;
                    case TranspositionEntryType.UpperBound:
                        if (ttValue < beta)
                            return ttValue;
                        break;
                }
            }
            pv = entry.PvMove;
        }

        ref MoveHistoryList moves = ref GetMoves(depth, pv);

        if (moves.Count < 1)
        {
            if (MoveHistory.GetPly() == _nullRootPly) return SearchValue;
            return MoveHistory.IsLastMoveWasCheck() ? GetMateNegativeValue() : 0;
        }

        int d = depth - 1;
        int b = NullWindow - beta;
        int best = MinusSearchValue;
        short bestKey = -1;
        byte i = 0;
        while (i < moves.Count && best < beta)
        {
            short key = moves[i++].Key;
            Position.MakeWhite(MoveProvider.Get(key));

            int value = -SearchBlackNull(b, d);

            Position.UnMakeWhite();

            if (value > best) { best = value; bestKey = key; }
        }

        Table.SetWhite(new TranspositionEntry((sbyte)depth, (short)ToTTValue(best), bestKey,
            best >= beta ? TranspositionEntryType.LowerBound : TranspositionEntryType.UpperBound));
        return best;
    }

    private int SearchBlackNull(int beta, int depth)
    {
        if (CheckDraw()) return 0;

        if (depth < 1) return EvaluateBlack(beta - NullWindow, beta);

        var entry = Table.GetBlack();
        short pv = -1;
        if (entry.Depth > 0)
        {
            if (entry.Depth >= depth)
            {
                int ttValue = FromTTValue(entry.Value);
                switch (entry.Type)
                {
                    case TranspositionEntryType.LowerBound:
                        if (ttValue >= beta)
                            return ttValue;
                        break;
                    case TranspositionEntryType.UpperBound:
                        if (ttValue < beta)
                            return ttValue;
                        break;
                }
            }
            pv = entry.PvMove;
        }

        ref MoveHistoryList moves = ref GetMoves(depth, pv);

        if (moves.Count < 1)
        {
            if (MoveHistory.GetPly() == _nullRootPly) return SearchValue;
            return MoveHistory.IsLastMoveWasCheck() ? GetMateNegativeValue() : 0;
        }

        int d = depth - 1;
        int b = NullWindow - beta;
        int best = MinusSearchValue;
        short bestKey = -1;
        byte i = 0;
        while (i < moves.Count && best < beta)
        {
            short key = moves[i++].Key;
            Position.MakeBlack(MoveProvider.Get(key));

            int value = -SearchWhiteNull(b, d);

            Position.UnMakeBlack();

            if (value > best) { best = value; bestKey = key; }
        }


        Table.SetBlack(new TranspositionEntry((sbyte)depth, (short)ToTTValue(best), bestKey,
           best >= beta ? TranspositionEntryType.LowerBound : TranspositionEntryType.UpperBound));
        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref MoveHistoryList GetMoves(int depth, short pv)
    {
        SortContext sortContext = DataPoolService.GetCurrentNullSortContext();
        if (pv < 0)
            sortContext.Set(Sorters[depth]);
        else
            sortContext.Set(Sorters[depth], pv);

        var context = DataPoolService.GetCurrentContext();
        context.Moves.Clear();
        sortContext.GetAllMoves(Position, ref context.Moves);
        return ref context.Moves;
    }
}
