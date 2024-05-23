using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;

namespace Engine.Sorting.Sorters;

public abstract class MoveSorter<T>:MoveSorterBase where T:AttackCollection
{
    protected static byte Zero = 0;
    protected T AttackCollection;

    protected MoveSorter(Position position):base(position)
    {
        InitializeMoveCollection();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetMoves() => AttackCollection.Build();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetBookMoves() => AttackCollection.BuildBook();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void AddSuggestedBookMove(MoveBase move) => AttackCollection.AddSuggestedBookMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCaptureMove(AttackBase attack)
    {
        attack.Captured = Board.GetPiece(attack.To);
        int attackValue = Board.StaticExchange(attack);
        if (attackValue > 0)
        {
            attack.See = attackValue;
            AttackCollection.AddWinCapture(attack);
        }
        else if (attackValue < 0)
        {
            attack.See = attackValue;
            AttackCollection.AddLooseCapture(attack);
        }
        else
        {
            AttackCollection.AddTrade(attack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionMoves(PromotionList promotions) => ProcessBlackPromotion(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionMoves(PromotionList promotions) => ProcessWhitePromotion(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionCaptures(PromotionAttackList moves)
    {
        Position.Make(moves[0]);

        AttackBase attack = Position.GetBlackAttackTo(moves[0].To);
        if (attack == null)
        {
            Position.UnMake();
            AddWinCapture(moves, Board.GetPiece(moves[0].To));
        }
        else
        {
            attack.Captured = WhitePawn;
            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                AttackCollection.AddWinCaptures(moves, see);
            }
            else
            {
                AttackCollection.AddLooseCaptures(moves, see);
            }
            Position.UnMake();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionCaptures(PromotionAttackList moves)
    {
        Position.Make(moves[0]);
        AttackBase attack = Position.GetWhiteAttackTo(moves[0].To);
        if (attack == null)
        {
            Position.UnMake();
            AddWinCapture(moves, Board.GetPiece(moves[0].To));
        }
        else
        {
            attack.Captured = BlackPawn;
            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                AttackCollection.AddWinCaptures(moves, see);
            }
            else
            {
                AttackCollection.AddLooseCaptures(moves, see);
            }
            Position.UnMake();
        }
    }

    protected abstract void InitializeMoveCollection();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ProcessBlackPromotion(PromotionList moves)
    {
        Position.Make(moves[0]);
        AttackBase attack =  Position.GetWhiteAttackTo(moves[0].To);
        if (attack == null)
        {
            AddWinCapture(moves);
        }
        else
        {
            attack.Captured = BlackPawn;
            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                AttackCollection.AddWinCaptures(moves, see);
            }
            else
            {
                AttackCollection.AddLooseCaptures(moves, see);
            }
        }
        Position.UnMake();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ProcessWhitePromotion(PromotionList moves)
    {
        Position.Make(moves[0]);
        
        AttackBase attack = Position.GetBlackAttackTo(moves[0].To);
        if (attack == null)
        {
            AddWinCapture(moves);
        }
        else
        {
            attack.Captured = WhitePawn;
            int see = -Board.StaticExchange(attack);

            if (see > 0)
            {
                AttackCollection.AddWinCaptures(moves, see);
            }
            else
            {
                AttackCollection.AddLooseCaptures(moves, see);
            }
        }
        Position.UnMake();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddWinCapture(PromotionList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            var move = moves[i];
            move.SetSee();
            AttackCollection.AddWinCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddWinCapture(PromotionAttackList moves, byte captured)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            var move = moves[i];
            move.SetSee(captured);
            AttackCollection.AddWinCapture(move);
        }
    }
}