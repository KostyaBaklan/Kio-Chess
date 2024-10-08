﻿using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Sorting.Sorters;

public abstract class MoveSorter<T> : MoveSorterBase where T : AttackCollection
{
    protected static byte Zero = 0;
    protected T AttackCollection;

    protected readonly BitBoard _minorStartPositions;
    protected readonly BitBoard _perimeter;

    protected MoveSorter(Position position) : base(position)
    {
        InitializeMoveCollection();
        _minorStartPositions = B1.AsBitBoard() | C1.AsBitBoard() | F1.AsBitBoard() |
                               G1.AsBitBoard() | B8.AsBitBoard() | C8.AsBitBoard() |
                               F8.AsBitBoard() | G8.AsBitBoard();
        _perimeter = Board.GetPerimeter();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMove(MoveBase move) => AttackCollection.AddHashMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionList promotions) => AttackCollection.AddHashMoves(promotions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionAttackList promotions) => AttackCollection.AddHashMoves(promotions);

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
    internal override void ProcessBlackPromotionMoves(PromotionList moves)
    {
        Position.MakeBlack(moves[0]);
        AttackBase attack = Board.GetWhiteAttackToForPromotion(moves[0].To);
        if (attack == null)
        {
            AddWinCapture(moves);
        }
        else
        {
            attack.Captured = BlackPawn;

            PromotionStaticExchange(moves, attack);
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
            AddWinCapture(moves);
        }
        else
        {
            attack.Captured = WhitePawn;

            PromotionStaticExchange(moves, attack);
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
            AddWinCapture(moves, Board.GetPiece(moves[0].To));
        }
        else
        {
            attack.Captured = WhitePawn;

            PromotionStaticExchange(moves, attack);
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
            AddWinCapture(moves, Board.GetPiece(moves[0].To));
        }
        else
        {
            attack.Captured = BlackPawn;
            PromotionStaticExchange(moves, attack);
            Position.UnMakeBlack();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PromotionStaticExchange(PromotionList moves, AttackBase attack)
    {
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PromotionStaticExchange(PromotionAttackList moves, AttackBase attack)
    {
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

    protected abstract void InitializeMoveCollection();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddWinCapture(PromotionList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            var move = moves[i];
            move.SetSee();
            AttackCollection.AddWinCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddWinCapture(PromotionAttackList moves, byte captured)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            var move = moves[i];
            move.SetSee(captured);
            AttackCollection.AddWinCapture(move);
        }
    }
}