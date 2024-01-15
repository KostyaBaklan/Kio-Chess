using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;

namespace Engine.Sorting.Sorters;

public abstract class ExtendedSorterBase<T> : CommonMoveSorter<T> where T : ExtendedMoveCollection
{
    protected readonly BitBoard _minorStartRanks;
    protected readonly BitBoard _whitePawnRank;
    protected readonly BitBoard _blackPawnRank;
    protected readonly PositionsList PositionsList;
    protected readonly AttackList Attacks;

    public ExtendedSorterBase(Position position) : base(position)
    {
        PositionsList = new PositionsList();
        Attacks = new AttackList();
        _minorStartRanks = Board.GetRank(0) | Board.GetRank(7);
        _whitePawnRank = Board.GetRank(2);
        _blackPawnRank = Board.GetRank(5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCounterMove(MoveBase move)
    {
        Position.Make(move);

        if (move.IsWhite)
        {
            if (IsBadAttackToWhite())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck && !Position.AnyBlackMoves())
            {
                AttackCollection.AddMateMove(move);
            }
            else
            {
                AttackCollection.AddCounterMove(move);
            }
        }
        else
        {
            if (IsBadAttackToBlack())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck && !Position.AnyWhiteMoves())
            {
                AttackCollection.AddMateMove(move);
            }
            else
            {
                AttackCollection.AddCounterMove(move);
            }
        }

        Position.UnMake();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessKillerMove(MoveBase move)
    {
        Position.Make(move);

        if (move.IsWhite)
        {
            if (IsBadAttackToWhite())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck && !Position.AnyBlackMoves())
            {
                AttackCollection.AddMateMove(move);
            }
            else
            {
                AttackCollection.AddKillerMove(move);
            }
        }
        else
        {
            if (IsBadAttackToBlack())
            {
                AttackCollection.AddNonSuggested(move);
            }
            else if (move.IsCheck && !Position.AnyWhiteMoves())
            {
                AttackCollection.AddMateMove(move);
            }
            else
            {
                AttackCollection.AddKillerMove(move);
            }
        }

        Position.UnMake();
    }

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
        Position.Make(attack);
        if (attack.IsCheck && !Position.AnyBlackMoves())
        {
            Position.UnMake();
            AttackCollection.AddMateMove(attack);
        }
        else
        {
            Position.UnMake();
            ProcessCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackCapture(AttackBase attack)
    {
        Position.Make(attack);
        if (attack.IsCheck && !Position.AnyWhiteMoves())
        {
            Position.UnMake();
            AttackCollection.AddMateMove(attack);
        }
        else
        {
            Position.UnMake();
            ProcessCaptureMove(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsGoodAttackForBlack()
    {
        GetBlackAttacks();
        return Attacks.Count > 0 && IsWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToBlack()
    {
        GetWhiteAttacks();
        return Attacks.Count > 0 && IsOpponentWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsGoodAttackForWhite()
    {
        GetWhiteAttacks();
        return Attacks.Count > 0 && IsWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsBadAttackToWhite()
    {
        GetBlackAttacks();
        return Attacks.Count > 0 && IsOpponentWinCapture();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsWinCapture()
    {
        for (byte i = 0; i < Attacks.Count; i++)
        {
            var attack = Attacks[i];
            attack.Captured = Board.GetPiece(attack.To);

            if (Board.StaticExchange(attack) > 0)
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

            if (Board.StaticExchange(attack) > 0)
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetBlackAttacks()
    {
        Attacks.Clear();
        if (Position.CanBlackPromote())
        {
            Position.GetBlackPromotionAttacks(Attacks);
        }
        Position.GetBlackAttacks(Attacks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetWhiteAttacks()
    {
        Attacks.Clear();
        if (Position.CanWhitePromote())
        {
            Position.GetWhitePromotionAttacks(Attacks);
        }

        Position.GetWhiteAttacks(Attacks);
    }
}