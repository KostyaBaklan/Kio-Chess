using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

/// <summary>
/// Partial class containing pawn race evaluation logic for endgames.
/// Calculates which side's passed pawn will promote first and awards bonus accordingly.
/// </summary>
public partial class Board
{
    /// <summary>
    /// Calculates the pawn race evaluation bonus for the current position.
    /// Returns positive value if white wins the race, negative if black wins.
    /// Returns 0 if no race exists or the race is unclear.
    /// </summary>
    /// <param name="isWhiteToMove">True if it's white's turn to move.</param>
    /// <returns>Pawn race bonus (positive = white advantage, negative = black advantage)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluatePawnRace(bool isWhiteToMove)
    {
        // Get the most advanced passed pawn for each side
        int whiteBestDistance = GetWhiteBestPassedPawnDistance();
        int blackBestDistance = GetBlackBestPassedPawnDistance();

        // No passed pawns means no race
        if (whiteBestDistance == int.MaxValue && blackBestDistance == int.MaxValue)
            return 0;

        // Only one side has passed pawns - no race, just evaluation of that pawn
        if (whiteBestDistance == int.MaxValue || blackBestDistance == int.MaxValue)
            return 0;

        // Account for tempo: the side to move effectively has 1 less distance
        if (isWhiteToMove)
        {
            whiteBestDistance--;
        }
        else
        {
            blackBestDistance--;
        }

        // Determine race winner
        // Note: After promotion, the promoted queen can often stop the opponent's pawn
        // So we need a clear tempo advantage (at least 2 moves) to consider it a "win"
        int advantage = blackBestDistance - whiteBestDistance;

        if (advantage >= 2)
        {
            // White wins the race decisively
            return _evaluationService.GetPawnRaceWinnerBonus();
        }
        else if (advantage <= -2)
        {
            // Black wins the race decisively
            return -_evaluationService.GetPawnRaceWinnerBonus();
        }
        else if (advantage == 1)
        {
            // White has slight advantage (promotes 1 move earlier)
            return _evaluationService.GetPawnRaceWinnerBonus() / 3;
        }
        else if (advantage == -1)
        {
            // Black has slight advantage
            return -_evaluationService.GetPawnRaceWinnerBonus() / 3;
        }

        // Race is equal
        return 0;
    }

    /// <summary>
    /// Gets the minimum distance to promotion for white's most advanced passed pawn.
    /// Takes into account whether the pawn can be stopped by the enemy king.
    /// </summary>
    /// <returns>Moves to promotion, or int.MaxValue if no unstoppable passed pawn exists.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBestPassedPawnDistance()
    {
        BitBoard whitePawns = _boards[Pieces.WhitePawn];
        BitBoard blackPawns = _boards[Pieces.BlackPawn];
        BitBoard allPawns = whitePawns | blackPawns;

        if (whitePawns.IsZero())
            return int.MaxValue;

        int bestDistance = int.MaxValue;

        while (whitePawns.Any())
        {
            byte coordinate = whitePawns.BitScanForward();
            whitePawns = whitePawns.Remove(coordinate);

            // Check if this is a passed pawn
            if ((_whiteFacing[coordinate] & allPawns).Any() ||
                (_whitePassedPawns[coordinate] & blackPawns).Any())
            {
                continue; // Not a passed pawn
            }

            // Calculate distance to promotion (8th rank = rank 7, squares 56-63)
            int rank = coordinate / 8;
            int distanceToPromotion = 7 - rank;

            // Check if path is clear (no blockers in front)
            bool pathClear = (_whiteFacing[coordinate] & _occupied).IsZero();

            if (pathClear)
            {
                // Check if enemy king can intercept using rule of the square
                if (!_whitePassedPawnSquare[coordinate].IsSet(_blackKingPosition))
                {
                    // Pawn is unstoppable - use exact distance
                    if (distanceToPromotion < bestDistance)
                    {
                        bestDistance = distanceToPromotion;
                    }
                }
                else
                {
                    // King might intercept - add penalty to distance
                    int effectiveDistance = distanceToPromotion + 2;
                    if (effectiveDistance < bestDistance)
                    {
                        bestDistance = effectiveDistance;
                    }
                }
            }
            else
            {
                // Path blocked - pawn needs captures or support
                int effectiveDistance = distanceToPromotion + 3;
                if (effectiveDistance < bestDistance)
                {
                    bestDistance = effectiveDistance;
                }
            }
        }

        return bestDistance;
    }

    /// <summary>
    /// Gets the minimum distance to promotion for black's most advanced passed pawn.
    /// Takes into account whether the pawn can be stopped by the enemy king.
    /// </summary>
    /// <returns>Moves to promotion, or int.MaxValue if no unstoppable passed pawn exists.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBestPassedPawnDistance()
    {
        BitBoard blackPawns = _boards[Pieces.BlackPawn];
        BitBoard whitePawns = _boards[Pieces.WhitePawn];
        BitBoard allPawns = whitePawns | blackPawns;

        if (blackPawns.IsZero())
            return int.MaxValue;

        int bestDistance = int.MaxValue;

        while (blackPawns.Any())
        {
            byte coordinate = blackPawns.BitScanForward();
            blackPawns = blackPawns.Remove(coordinate);

            // Check if this is a passed pawn
            if ((_blackFacing[coordinate] & allPawns).Any() ||
                (_blackPassedPawns[coordinate] & whitePawns).Any())
            {
                continue; // Not a passed pawn
            }

            // Calculate distance to promotion (1st rank = rank 0, squares 0-7)
            int rank = coordinate / 8;
            int distanceToPromotion = rank;

            // Check if path is clear (no blockers in front)
            bool pathClear = (_blackFacing[coordinate] & _occupied).IsZero();

            if (pathClear)
            {
                // Check if enemy king can intercept using rule of the square
                if (!_blackPassedPawnSquare[coordinate].IsSet(_whiteKingPosition))
                {
                    // Pawn is unstoppable - use exact distance
                    if (distanceToPromotion < bestDistance)
                    {
                        bestDistance = distanceToPromotion;
                    }
                }
                else
                {
                    // King might intercept - add penalty to distance
                    int effectiveDistance = distanceToPromotion + 2;
                    if (effectiveDistance < bestDistance)
                    {
                        bestDistance = effectiveDistance;
                    }
                }
            }
            else
            {
                // Path blocked - pawn needs captures or support
                int effectiveDistance = distanceToPromotion + 3;
                if (effectiveDistance < bestDistance)
                {
                    bestDistance = effectiveDistance;
                }
            }
        }

        return bestDistance;
    }
}
