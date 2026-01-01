using Engine.Models.Boards;
using Engine.Models.Enums;
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
                MoveCollection.AddLooseMajorPiece(move);
                return true;
            }
        }

        for (byte i = 0; i < MinorLooseAttacks.Count; i++)
        {
            if (Board.StaticExchangeWithPins(MinorLooseAttacks[i]) > 0)
            {
                MoveCollection.AddLooseMinorPiece(move);
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
                    MoveCollection.AddLooseCheck(move);
                }
                else if (Board.AnyBlackKingMovesOnCheck())
                {
                    MoveCollection.AddSuggested(move);
                }
                else
                {
                    MoveCollection.AddMateMove(move);
                }
            }
            else //discovered
            {
                var attack = Board.GetBlackAttackToForCheck(bit.BitScanForward());
                if (attack != null && Board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    MoveCollection.AddLooseCheck(move);
                }
                else if (Position.AnyBlackMoves())
                {
                    MoveCollection.AddSuggested(move);
                }
                else
                {
                    MoveCollection.AddMateMove(move);
                }
            }

            return true;
        }
        if (Position.AnySuccessfullBlackPromotion())
        {
            MoveCollection.AddMissedEnemyPromotions(move);
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
                    MoveCollection.AddLooseCheck(move);
                }
                else if (Board.AnyWhiteKingMovesOnCheck())
                {
                    MoveCollection.AddSuggested(move);
                }
                else
                {
                    MoveCollection.AddMateMove(move);
                }
            }
            else //discovered
            {
                var attack = Board.GetWhiteAttackToForCheck(bit.BitScanForward());
                if (attack != null && Board.StaticExchangeWithPinsWithoutTarget(attack) > 0)
                {
                    MoveCollection.AddLooseCheck(move);
                }
                else if (Position.AnyWhiteMoves())
                {
                    MoveCollection.AddSuggested(move);
                }
                else
                {
                    MoveCollection.AddMateMove(move);
                }
            }


            return true;
        }
        if (Position.AnySuccessfullWhitePromotion())
        {
            MoveCollection.AddMissedEnemyPromotions(move);
            return true;
        }

        return IsBadAttackToBlack(move);
    }
}
