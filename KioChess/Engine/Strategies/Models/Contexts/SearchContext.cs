using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts;


public class SearchContext
{
    internal SearchResultType SearchResultType;

    internal int Value;
    internal int Ply;

    internal MoveList Moves;
    internal short BestMove;
    internal KillerMoves CurrentKillers;
    internal bool[] LowSee;
    public static MoveHistoryService MoveHistory;
    internal static Position Position;

    public static int MateNegative;
    public static sbyte RazoringDepth;

    public static int[][] AlphaMargins;
    public static int[][] BetaMargins;

    public SearchContext()
    {
        Value = short.MinValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Value = short.MinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(short move)
    {
        CurrentKillers.Add(move);
        MoveHistory.SetCounterMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetSearchResultType(int alpha, int beta, sbyte depth)
    {
        if (Moves.Count < 1)
        {
            SearchResultType = SearchResultType.EndGame;
            Value = MoveHistory.IsLastMoveWasCheck() ? MateNegative : 0;
        }
        else
        {
            SearchResultType = depth > RazoringDepth || MoveHistory.IsLastMoveWasCheck()
                ? SearchResultType.None
                : SetEndGameType(alpha, beta, depth);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SearchResultType SetEndGameType(int alpha, int beta, sbyte depth)
    {
        int value = Position.GetStaticValue();

        byte phase = MoveHistory.GetPhase();

        if (depth < RazoringDepth)
        {
            if (value + AlphaMargins[phase][depth] < alpha) return SearchResultType.AlphaFutility;
            if (value - BetaMargins[phase][depth] > beta) return SearchResultType.BetaFutility;
            return SearchResultType.None;
        }

        return value + AlphaMargins[phase][depth] < alpha ? SearchResultType.Razoring : SearchResultType.None;
    }
}
