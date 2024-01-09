using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public abstract class CommonMoveSorter : MoveSorter 
{
    protected readonly BitBoard _minorStartPositions;
    protected readonly BitBoard _perimeter;
    protected CommonMoveSorter(IPosition position) : base(position)
    {
        _minorStartPositions = B1.AsBitBoard() | C1.AsBitBoard() | F1.AsBitBoard() |
                               G1.AsBitBoard() | B8.AsBitBoard() | C8.AsBitBoard() |
                               F8.AsBitBoard() | G8.AsBitBoard();
        _perimeter = Board.GetPerimeter();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningMove(MoveBase move)
    {
        AddNonCapture(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackPromotionMoves(PromotionList promotions)
    {
        ProcessBlackPromotion(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhitePromotionMoves(PromotionList promotions)
    {
        ProcessWhitePromotion(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionList promotions)
    {
        MoveValueList.AddHashMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessHashMoves(PromotionAttackList promotions)
    {
        MoveValueList.AddHashMoves(promotions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteOpeningCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteMiddleCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackOpeningCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackMiddleCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndCapture(AttackBase move)
    {
        ProcessCaptureMove(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddNonCapture(MoveBase move)
    {
        if (move.IsForward[Phase])
            MoveValueList.AddForwardMove(move);
        else
            MoveValueList.AddNonCapture(move);
    }
}
