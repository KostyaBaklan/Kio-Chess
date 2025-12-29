using Engine.Models.Boards;
using Engine.Models.Enums;
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
        attack.SetCapturedPiece();
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
                var bit = Board.GetBlackKingAttackPositions();

                int maxSee = short.MinValue;

                Action<AttackBase> action = a =>
                {
                    var see = Board.StaticExchangeWithPinsWithoutTarget(a);
                    if (see > maxSee)
                    {
                        maxSee = see;
                    }
                };

                MoveProvider.GetBlackKingAttacks(Board.GetPieceBits(Pieces.BlackKing), action);

                if (bit.Count() < 2) //double check. Only king moves possible
                {
                    Board.GenerateBlackAttacksTo(bit.BitScanForward(), action);
                }

                var capturedValue = attack.GetCapturedValue();

                Position.UnMakeWhite();

                if (maxSee > short.MinValue)
                {
                    ClassifyBlackCheckAttack(attack, maxSee - capturedValue);
                }
                else
                {
                    attack.See = capturedValue;
                    AttackCollection.AddWinCapture(attack);
                }

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
        attack.SetCapturedPiece();
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
                var bit = Board.GetWhiteKingAttackPositions();
                int maxSee = short.MinValue;

                Action<AttackBase> action = a =>
                {
                    var see = Board.StaticExchangeWithPinsWithoutTarget(a);
                    if (see > maxSee)
                    {
                        maxSee = see;
                    }
                };

                MoveProvider.GetWhiteKingAttacks(Board.GetPieceBits(Pieces.WhiteKing), action);

                if (bit.Count() < 2) //double check. Only king moves possible
                {
                    Board.GenerateWhiteAttacksTo(bit.BitScanForward(), action);
                }

                var capturedValue = attack.GetCapturedValue();

                Position.UnMakeBlack();

                if (maxSee > short.MinValue)
                {
                    ClassifyWhiteCheckAttack(attack, maxSee - capturedValue);
                }
                else
                {
                    attack.See = capturedValue;
                    AttackCollection.AddWinCapture(attack);
                }

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
    private void ProcessWhiteCaptureMove(AttackBase attack)
    {
        attack.SetCapturedPiece();
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
        attack.SetCapturedPiece();
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
    private void AddLooseCapture(AttackBase attack)
    {
        if (attack.IsCheck)
        {
            AttackCollection.AddLooseCheckAttack(attack);
        }
        else
        {
            AttackCollection.AddLooseCapture(attack);
        }
    }
}