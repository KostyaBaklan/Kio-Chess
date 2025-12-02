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
    internal override void ProcessWhiteOpeningCapture(AttackBase attack) => ProcessWhiteCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleCapture(AttackBase attack) => ProcessWhiteCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndCapture(AttackBase attack) => ProcessWhiteCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningCapture(AttackBase attack) => ProcessBlackCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleCapture(AttackBase attack) => ProcessBlackCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndCapture(AttackBase attack) => ProcessBlackCapture(attack);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCapture(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        Position.MakeWhite(attack);
        if (attack.IsCheck)
        {
            if (!Position.AnyBlackMoves())
            {
                Position.UnMakeWhite();
                AttackCollection.AddMateMove(attack);
            }
            else
            {
                var capturedValue = attack.GetCapturedValue();
                var bit = Board.GetBlackKingAttackPositions();
                Attacks.Clear();
                if (bit.Count() > 1) //double check. Only king moves possible
                {
                    MoveProvider.GetBlackKingAttacks(Board.GetPieceBits(Pieces.BlackKing), Attacks);
                }
                else
                {
                    Position.GetBlackAttacks(Attacks);
                }
                ProcessBlackAttackOnCheck(attack, capturedValue);
                LowSee[attack.Key] = false;
            }
        }
        else
        {
            Position.UnMakeWhite();
            ProcessWhiteCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapture(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        Position.MakeBlack(attack);
        if (attack.IsCheck)
        {
            if (!Position.AnyWhiteMoves())
            {
                Position.UnMakeBlack();
                AttackCollection.AddMateMove(attack);
            }
            else
            {
                var capturedValue = attack.GetCapturedValue();
                var bit = Board.GetWhiteKingAttackPositions();
                Attacks.Clear();
                if (bit.Count() > 1) //double check. Only king moves possible
                {
                    MoveProvider.GetWhiteKingAttacks(Board.GetPieceBits(Pieces.WhiteKing), Attacks);
                }
                else
                {
                    Position.GetWhiteAttacks(Attacks);
                }
                ProcessWhiteAttackOnCheck(attack, capturedValue);
                LowSee[attack.Key] = false;
            }
        }
        else
        {
            Position.UnMakeBlack();
            ProcessBlackCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackAttackOnCheck(AttackBase attack, int capturedValue)
    {
        int maxSee = GetMaxSee();

        Position.UnMakeBlack();

        if (maxSee > short.MinValue)
        {
            ClassifyBlackCheckAttack(attack, maxSee - capturedValue);
        }
        else
        {
            attack.See = capturedValue;
            AttackCollection.AddWinCapture(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteAttackOnCheck(AttackBase attack, int capturedValue)
    {
        int maxSee = GetMaxSee();

        Position.UnMakeWhite();

        if (maxSee > short.MinValue)
        {
            ClassifyWhiteCheckAttack(attack, maxSee - capturedValue);
        }
        else
        {
            attack.See = capturedValue;
            AttackCollection.AddWinCapture(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        int attackValue = Board.StaticExchangeWithPins(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            AttackCollection.AddWinCapture(attack);
            LowSee[attack.Key] = false;
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            if (!attack.IsCheck)
            {
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = true;
            }
            else
            {
                AttackCollection.AddLooseCheckAttack(attack);
                LowSee[attack.Key] = false;
            }
        }
        else
        {
            ProcessWhiteTrade(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        int attackValue = Board.StaticExchangeWithPins(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            AttackCollection.AddWinCapture(attack);
            LowSee[attack.Key] = false;
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            if (!attack.IsCheck)
            {
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = true;
            }
            else
            {
                AttackCollection.AddLooseCheckAttack(attack);
                LowSee[attack.Key] = false;
            }
        }
        else
        {
            ProcessBlackTrade(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackTrade(AttackBase attack)
    {
        if (StaticValue < _minusTradeMargin)
        {
            attack.See = 0;
            AddLooseCapture(attack);
            LowSee[attack.Key] = false;
        }
        else if (StaticValue > _tradeMargin)
        {
            attack.See = 0;
            AttackCollection.AddWinCapture(attack);
            LowSee[attack.Key] = false;
        }
        else
        {
            if (attack.Piece == Pieces.BlackBishop && Board.GetPieceBits(Pieces.BlackBishop).Count() > 1 && attack.Captured == Pieces.WhiteKnight)
            {
                attack.See = -50;
                AddLooseCapture(attack);
                LowSee[attack.Key] = false;
            }
            else if (attack.Piece == Pieces.BlackKnight && attack.Captured == Pieces.WhiteBishop && Board.GetPieceBits(Pieces.WhiteBishop).Count() > 1)
            {
                attack.See = 50;
                AttackCollection.AddWinCapture(attack);
                LowSee[attack.Key] = false;
            }
            else
            {
                AttackCollection.AddTrade(attack);
                LowSee[attack.Key] = false;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteTrade(AttackBase attack)
    {
        if (StaticValue < _minusTradeMargin)
        {
            attack.See = 0;
            AddLooseCapture(attack);
            LowSee[attack.Key] = false;
        }
        else if (StaticValue > _tradeMargin)
        {
            attack.See = 0;
            AttackCollection.AddWinCapture(attack);
            LowSee[attack.Key] = false;
        }
        else
        {
            if (attack.Piece == Pieces.WhiteBishop && Board.GetPieceBits(Pieces.WhiteBishop).Count() > 1 && attack.Captured == Pieces.BlackKnight)
            {
                attack.See = -50;
                AddLooseCapture(attack);
                LowSee[attack.Key] = false;
            }
            else if (attack.Piece == Pieces.WhiteKnight && attack.Captured == Pieces.BlackBishop && Board.GetPieceBits(Pieces.BlackBishop).Count() > 1)
            {
                attack.See = 50;
                AttackCollection.AddWinCapture(attack);
                LowSee[attack.Key] = false;
            }
            else
            {
                AttackCollection.AddTrade(attack);
                LowSee[attack.Key] = false;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClassifyWhiteCheckAttack(AttackBase attack, int see)
    {
        attack.See = see;
        if (see > 0)
        {
            AttackCollection.AddLooseCheckAttack(attack);
        }
        else if (see < 0)
        {
            AttackCollection.AddWinCapture(attack);
        }
        else
        {
            ProcessWhiteTrade(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClassifyBlackCheckAttack(AttackBase attack, int see)
    {
        attack.See = see;
        if (see > 0)
        {
            AttackCollection.AddLooseCheckAttack(attack);
        }
        else if (see < 0)
        {
            AttackCollection.AddWinCapture(attack);
        }
        else
        {
            ProcessBlackTrade(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetMaxSee()
    {
        int maxSee = short.MinValue;
        if (Attacks.Count > 0)
        {
            for (byte i = 0; i < Attacks.Count; i++)
            {
                var a = Attacks[i];
                a.Captured = Board.GetPiece(a.To);
                var see = Board.StaticExchangeWithPins(a);
                if (see > maxSee)
                {
                    maxSee = see;
                }
            }

        }

        return maxSee;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLooseCapture(AttackBase attack)
    {
        if(attack.IsCheck)
        {
            AttackCollection.AddLooseCheckAttack(attack);
        }
        else
        {
            AttackCollection.AddLooseCapture(attack);
        }
    }
}