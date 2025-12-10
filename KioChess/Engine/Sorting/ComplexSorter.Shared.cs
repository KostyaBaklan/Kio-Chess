using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
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
            if (Board.StaticExchangeWithPins(MajorLooseAttacks[i]) > 0)
            {
                AttackCollection.AddLooseMajorPiece(move);
                return true;
            }
        }

        for (byte i = 0; i < MinorLooseAttacks.Count; i++)
        {
            if (Board.StaticExchangeWithPins(MinorLooseAttacks[i]) > 0)
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
                if (attack != null && Board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
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
        if (Position.AnySuccessfullBlackPromotion())
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
                if (attack != null && Board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
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
        if (Position.AnySuccessfullWhitePromotion())
        {
            AttackCollection.AddMissedEnemyPromotions(move);
            return true;
        }

        return IsBadAttackToBlack(move);
    }
}
