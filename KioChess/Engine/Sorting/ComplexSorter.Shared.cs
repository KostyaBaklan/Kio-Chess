using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToWhite(MoveBase move)
    {
        ClearAttacks();

        Position.GetBlackAttacks(Attacks);

        for (byte i = 0; i < Attacks.Count; i++)
        {
            var attack = Attacks[i];
            byte captured = Board.GetPiece(attack.To);
            attack.Captured = captured;

            if (captured == Pieces.WhiteRook || captured == Pieces.WhiteQueen)
            {
                MajorLooseAttacks.Add(attack);
            }
            else
            {
                MinorLooseAttacks.Add(attack);
            }
        }

        return IsBadAttack(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToBlack(MoveBase move)
    {
        ClearAttacks();

        Position.GetWhiteAttacks(Attacks);

        for (byte i = 0; i < Attacks.Count; i++)
        {
            var attack = Attacks[i];
            byte captured = Board.GetPiece(attack.To);
            attack.Captured = captured;

            if (captured == Pieces.BlackRook || captured == Pieces.BlackQueen)
            {
                MajorLooseAttacks.Add(attack);
            }
            else
            {
                MinorLooseAttacks.Add(attack);
            }
        }

        return IsBadAttack(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearAttacks()
    {
        Attacks.Clear();
        MinorLooseAttacks.Clear();
        MajorLooseAttacks.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBadAttack(MoveBase move)
    {
        for (byte i = 0; i < MajorLooseAttacks.Count; i++)
        {
            var attack = Attacks[i];
            if (Board.StaticExchangeWithPins(attack) > 0)
            {
                AttackCollection.AddLooseMajorPiece(move);
                return true;
            }
        }

        for (byte i = 0; i < MinorLooseAttacks.Count; i++)
        {
            var attack = Attacks[i];
            if (Board.StaticExchangeWithPins(attack) > 0)
            {
                AttackCollection.AddLooseMinorPiece(move);
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckWhiteResult(MoveBase move)
    {
        if (move.IsCheck)
        {
            var bit = Board.GetBlackKingAttackPositions();

            if (bit.Count() > 1) //double
            {
                if (Board.AnyBlackKingAttacksOnCheck())
                {
                    AttackCollection.AddLooseCheck(move);
                }
                else if (Board.AnyBlackKingMovesOnCheck())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else //discovered
            {
                var attack = Board.GetBlackAttackToForCheck(bit.BitScanForward());
                if (attack != null && Board.StaticExchangeWithPins(attack) > 0)
                {
                    AttackCollection.AddLooseCheck(move);
                }
                else if (Position.AnyBlackMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }

            return true;
        }
        if (IsMissedBlackPromotion())
        {
            AttackCollection.AddMissedEnemyPromotions(move);
            return true;
        }

        return IsBadAttackToWhite(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckBlackResult(MoveBase move)
    {
        if (move.IsCheck)
        {
            var bit = Board.GetWhiteKingAttackPositions();
            if (bit.Count() > 1) //double
            {
                if (Board.AnyWhiteKingAttacksOnCheck())
                {
                    AttackCollection.AddLooseCheck(move);
                }
                else if (Board.AnyWhiteKingMovesOnCheck())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }
            else //discovered
            {
                var attack = Board.GetWhiteAttackToForCheck(bit.BitScanForward());
                if (attack != null && Board.StaticExchangeWithPins(attack) > 0)
                {
                    AttackCollection.AddLooseCheck(move);
                }
                else if (Position.AnyWhiteMoves())
                {
                    AttackCollection.AddSuggested(move);
                }
                else
                {
                    AttackCollection.AddMateMove(move);
                }
            }


            return true;
        }
        if (IsMissedWhitePromotion())
        {
            AttackCollection.AddMissedEnemyPromotions(move);
            return true;
        }

        return IsBadAttackToBlack(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsMissedWhitePromotion()
    {
        if (Board.CanWhitePromote())
        {
            var board = Board.GetWhitePromotionSquares();

            while (board.Any())
            {
                var f = board.BitScanForward();

                var promotions = MoveProvider.GetWhitePromotionAttacks(f);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && Board.IsWhiteMoveLigal(promotions[i][0]))
                    {
                        return true;
                        //PromotionAttack whitePromotionAttack = promotions[i][0];

                        //Position.MakeWhite(whitePromotionAttack);
                        //AttackBase attack = Board.GetBlackAttackToForPromotion(whitePromotionAttack.To);
                        //Position.UnMakeWhite();
                        //if (attack == null)
                        //{
                        //    return true;
                        //}
                        //else
                        //{
                        //    //attack.Captured = Pieces.BlackPawn;
                        //    //int see = -Board.StaticExchangeWithPins(attack);
                        //    //Position.UnMakeBlack();
                        //    //if (see > 0)
                        //    //{
                        //    //    return true;
                        //    //}
                        //}
                    }

                    var p = MoveProvider.GetWhitePromotions(f);

                    if (p.Count > 0 && Board.IsWhiteMoveLigal(p[0]))
                    {
                        PromotionMove whitePromotion = p[0];

                        Position.MakeWhite(whitePromotion);
                        AttackBase attack = Board.GetBlackAttackToForPromotion(whitePromotion.To);
                        Position.UnMakeWhite();
                        if (attack == null)
                        {
                            return true;
                        }
                    }

                    board = board.Remove(f);
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsMissedBlackPromotion()
    {
        if (Board.CanBlackPromote())
        {
            var board = Board.GetBlackPromotionSquares();

            while (board.Any())
            {
                var f = board.BitScanForward();

                var promotions = MoveProvider.GetBlackPromotionAttacks(f);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && Board.IsBlackMoveLigal(promotions[i][0]))
                    {
                        return true;
                        //PromotionAttack blackPromotionAttack = promotions[i][0];

                        //Position.MakeBlack(blackPromotionAttack);
                        //AttackBase attack = Board.GetWhiteAttackToForPromotion(blackPromotionAttack.To);
                        //Position.UnMakeBlack();
                        //if (attack == null)
                        //{
                        //    return true;
                        //}
                        //else
                        //{
                        //    //attack.Captured = Pieces.BlackPawn;
                        //    //int see = -Board.StaticExchangeWithPins(attack);
                        //    //Position.UnMakeBlack();
                        //    //if (see > 0)
                        //    //{
                        //    //    return true;
                        //    //}
                        //}
                    }

                    var p = MoveProvider.GetBlackPromotions(f);

                    if (p.Count > 0 && Board.IsBlackMoveLigal(p[0]))
                    {
                        PromotionMove blackPromotion = p[0];

                        Position.MakeBlack(blackPromotion);
                        AttackBase attack = Board.GetWhiteAttackToForPromotion(blackPromotion.To);
                        Position.UnMakeBlack();
                        if (attack == null)
                        {
                            return true;
                        }
                    }

                    board = board.Remove(f);
                }
            }
        }
        return false;
    }
}
