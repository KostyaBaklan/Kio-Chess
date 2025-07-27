using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsWinCapture()
    {
        for (byte i = 0; i < Attacks.Count; i++)
        {
            var attack = Attacks[i];
            attack.Captured = Board.GetPiece(attack.To);
            if (Board.StaticExchangeWithPins(attack) > 0)
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsOpponentWinCapture()
    {
        for (byte i = 0; i < Attacks.Count; i++)
        {
            var attack = Attacks[i];
            attack.Captured = Board.GetPiece(attack.To);
            if (Board.StaticExchangeWithPins(attack) > 0)
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetBlackAttacks()
    {
        Attacks.Clear();
        if (Board.CanBlackPromote())
        {
            Position.GetBlackPromotionAttacks(Attacks);
        }
        Position.GetBlackAttacks(Attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetWhiteAttacks()
    {
        Attacks.Clear();
        if (Board.CanWhitePromote())
        {
            Position.GetWhitePromotionAttacks(Attacks);
        }
        Position.GetWhiteAttacks(Attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToWhite()
    {
        GetBlackAttacks();
        return IsOpponentWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToBlack()
    {
        GetWhiteAttacks();
        return IsOpponentWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckWhiteResult(MoveBase move)
    {
        if (move.IsCheck)
        {
            var attack = Board.GetBlackAttackToForCheck(move.To);
            if (attack != null && Board.StaticExchangeWithPins(attack) > 0)
            {
                AttackCollection.AddLooseCheck(move);
            }
            else
            {
                if (Position.AnyBlackMoves())
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
        if (IsBadAttackToWhite())
        {
            AttackCollection.AddLooseNonCapture(move);
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckBlackResult(MoveBase move)
    {
        if (move.IsCheck)
        {
            var attack = Board.GetWhiteAttackToForCheck(move.To);
            if (attack != null && Board.StaticExchangeWithPins(attack) > 0)
            {
                AttackCollection.AddLooseCheck(move);
            }
            else
            {
                if (Position.AnyWhiteMoves())
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
        if (IsBadAttackToBlack())
        {
            AttackCollection.AddLooseNonCapture(move);
            return true;
        }
        return false;
    }
}
