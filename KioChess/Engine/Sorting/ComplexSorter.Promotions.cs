using Engine.DataStructures.Moves.Lists;
using Engine.Models.Enums;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionMoves(PromotionList moves)
    {
        Position.MakeBlack(moves[0]);
        AttackBase attack = Board.GetWhiteAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                move.SetSee();
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.BlackPawn;

            int see = -Board.StaticExchangeWithPins(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee.Add(move.LowSeeKey);
                    AttackCollection.AddLooseCapture(move);
                }
            }
        }
        Position.UnMakeBlack();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionMoves(PromotionList moves)
    {
        Position.MakeWhite(moves[0]);

        AttackBase attack = Board.GetBlackAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                move.SetSee();
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.WhitePawn;

            int see = -Board.StaticExchangeWithPins(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee.Add(move.LowSeeKey);
                    AttackCollection.AddLooseCapture(move);
                }
            }
        }
        Position.UnMakeWhite();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionCaptures(PromotionAttackList moves)
    {
        Position.MakeWhite(moves[0]);

        AttackBase attack = Board.GetBlackAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            Position.UnMakeWhite();
            var captured = Board.GetPiece(moves[0].To);
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                move.SetSee(captured);
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.WhitePawn;

            int see = -Board.StaticExchangeWithPins(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee.Add(move.LowSeeKey);
                    AttackCollection.AddLooseCapture(move);
                }
            }
            Position.UnMakeWhite();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionCaptures(PromotionAttackList moves)
    {
        Position.MakeBlack(moves[0]);
        AttackBase attack = Board.GetWhiteAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            Position.UnMakeBlack();
            var captured = Board.GetPiece(moves[0].To);
            for (byte i = Zero; i < moves.Count; i++)
            {
                var move = moves[i];
                move.SetSee(captured);
                AttackCollection.AddWinCapture(move);
            }
        }
        else
        {
            attack.Captured = Pieces.BlackPawn;

            int see = -Board.StaticExchangeWithPins(attack);

            if (see > 0)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    AttackCollection.AddWinCapture(move);
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    move.See = see;
                    LowSee.Add(move.LowSeeKey);
                    AttackCollection.AddLooseCapture(move);
                }
            }
            Position.UnMakeBlack();
        }
    }
}