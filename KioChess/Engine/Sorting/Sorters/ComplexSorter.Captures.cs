using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
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
        Position.MakeWhite(attack);
        if (attack.IsCheck)
        {
            if (!Position.AnyBlackMoves())
            {
                Position.UnMakeWhite();
                AttackCollection.AddMateMove(attack);
            }
            else if (!Board.AnyBlackAttackTo(attack.To))
            {
                Position.UnMakeWhite();
                attack.SetCapturedValue();
                AttackCollection.AddWinCapture(attack);
            }
            else
            {
                Position.UnMakeWhite();
                ProcessWhiteCaptureMove(attack);
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
        Position.MakeBlack(attack);
        if (attack.IsCheck)
        {
            if (!Position.AnyWhiteMoves())
            {
                Position.UnMakeBlack();
                AttackCollection.AddMateMove(attack);
            }
            else if (!Board.AnyWhiteAttackTo(attack.To))
            {
                Position.UnMakeBlack();
                attack.SetCapturedValue();
                AttackCollection.AddWinCapture(attack);
            }
            else
            {
                Position.UnMakeBlack();
                ProcessBlackCaptureMove(attack);
            }
        }
        else
        {
            Position.UnMakeBlack();
            ProcessBlackCaptureMove(attack);
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
            if (StaticValue < _minusTradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = false;
            }
            else if (StaticValue > _tradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
                LowSee[attack.Key] = false;
            }
            else
            {
                if (attack.Piece == Pieces.WhiteBishop && Board.GetPieceBits(Pieces.WhiteBishop).Count() > 1 && attack.Captured == Pieces.BlackKnight)
                {
                    attack.See = -50;
                    AttackCollection.AddLooseCapture(attack);
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
            if (StaticValue < _minusTradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
                LowSee[attack.Key] = false;
            }
            else if (StaticValue > _tradeMargin)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
                LowSee[attack.Key] = false;
            }
            else
            {
                if (attack.Piece == Pieces.BlackBishop && Board.GetPieceBits(Pieces.BlackBishop).Count() > 1 && attack.Captured == Pieces.WhiteKnight)
                {
                    attack.See = -50;
                    AttackCollection.AddLooseCapture(attack);
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
    }
}