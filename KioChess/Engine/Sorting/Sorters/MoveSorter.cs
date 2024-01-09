using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.Sorting.Sorters;

public abstract class MoveSorter:MoveSorterBase
{
    protected MoveSorter(IPosition position):base(position)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void AddSuggestedBookMove(MoveBase move)
    {
        MoveValueList.AddSuggestedBookMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        short attackValue = Board.StaticExchange(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            MoveValueList.AddWinCapture(attack);
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            MoveValueList.AddLooseCapture(attack);
        }
        else
        {
            MoveValueList.AddTrade(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions)
    {
        ProcessPromotionCaptures(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions)
    {
        ProcessPromotionCaptures(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ProcessPromotionCaptures(PromotionAttackList promotions)
    {
        var attack = promotions[0];
        attack.Captured = Board.GetPiece(attack.To);

        short attackValue = Board.StaticExchange(attack);
        if (attackValue > 0)
        {
            MoveValueList.AddWinCaptures(promotions,attackValue);
        }
        else
        {
            MoveValueList.AddLooseCapture(promotions, attackValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ProcessBlackPromotion(PromotionList moves)
    {
        Position.Make(moves[0]);
        Position.GetWhiteAttacksTo(moves[0].To, attackList);
        StaticBlackExchange(moves);
        Position.UnMake();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ProcessWhitePromotion(PromotionList moves)
    {
        Position.Make(moves[0]);
        Position.GetBlackAttacksTo(moves[0].To, attackList);
        StaticWhiteExchange(moves);
        Position.UnMake();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void StaticWhiteExchange(PromotionList moves)
    {
        if (attackList.Count == 0)
        {
            MoveValueList.AddWinCapture(moves);
        }
        else
        {
            WhitePromotion(moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void StaticBlackExchange(PromotionList moves)
    {
        if (attackList.Count == 0)
        {
            MoveValueList.AddWinCapture(moves);
        }
        else
        {
            BlackPromotion(moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WhitePromotion(PromotionList moves)
    {
        short max = short.MinValue;
        for (byte i = 0; i < attackList.Count; i++)
        {
            var attack = attackList[i];
            attack.Captured = WhitePawn;
            short see = Board.StaticExchange(attack);
            if (see > max)
            {
                max = see;
            }
        }

        if (max < 0)
        {
            MoveValueList.AddWinCapture(moves);
        }
        else
        {
            MoveValueList.AddLooseCapture(moves);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlackPromotion(PromotionList moves)
    {
        short max = short.MinValue;
        for (byte i = 0; i < attackList.Count; i++)
        {
            var attack = attackList[i];
            attack.Captured = BlackPawn;
            short see = Board.StaticExchange(attack);
            if (see > max)
            {
                max = see;
            }
        }

        if (max < 0)
        {
            MoveValueList.AddWinCapture(moves);
        }
        else
        {
            MoveValueList.AddLooseCapture(moves);
        }
    }
}