using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Services;

public partial class MoveProvider
{
    #region Promotions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionList GetWhitePromotions(byte from)
    {
        BitBoard board = (from.AsBitBoard() << 8) & _board.GetEmpty();

        return board.Any() ? _whitePromotions[from][board.BitScanForward()] : _emptyPromotions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionList GetBlackPromotions(byte from)
    {
        BitBoard board = (from.AsBitBoard() >> 8) & _board.GetEmpty();

        return board.Any() ? _blackPromotions[from][board.BitScanForward()] : _emptyPromotions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttack GetWhitePromotionAttack(byte from, byte to)
    {
        BitBoard board = _whitePawnPatterns[from] & _board.GetBlacks();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            if (position == to)
            {
                return _whitePromotionAttacks[from][position][0];
            }
            board = board.Remove(position);

            if (board.Any())
            {
                return _whitePromotionAttacks[from][board.BitScanForward()][0];
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttack GetBlackPromotionAttack(byte from, byte to)
    {
        BitBoard board = _blackPawnPatterns[from] & _board.GetWhites();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            if (position == to)
            {
                return _blackPromotionAttacks[from][position][0];
            }
            board = board.Remove(position);

            if (board.Any())
            {
                return _blackPromotionAttacks[from][board.BitScanForward()][0];
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttackPair GetWhitePromotionAttacks(byte from)
    {
        PromotionAttackPair promotions = new PromotionAttackPair { First = _emptyPromotionAttacks, Second = _emptyPromotionAttacks };

        BitBoard board = _whitePawnPatterns[from] & _board.GetBlacks();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            promotions.First = _whitePromotionAttacks[from][position];
            board = board.Remove(position);

            if (board.Any())
            {
                promotions.Second = _whitePromotionAttacks[from][board.BitScanForward()];
            }
        }

        return promotions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttackPair GetBlackPromotionAttacks(byte from)
    {
        PromotionAttackPair promotions = new PromotionAttackPair { First = _emptyPromotionAttacks, Second = _emptyPromotionAttacks };

        BitBoard board = _blackPawnPatterns[from] & _board.GetWhites();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            promotions.First = _blackPromotionAttacks[from][position];
            board = board.Remove(position);

            if (board.Any())
            {
                promotions.Second = _blackPromotionAttacks[from][board.BitScanForward()];
            }
        }

        return promotions;
    }

    #endregion
}
