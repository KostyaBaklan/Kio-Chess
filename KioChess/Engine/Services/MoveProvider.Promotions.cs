using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards.Structures;
using Engine.Models.Helpers;
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
    public PromotionAttackList[] GetWhitePromotionAttacks(byte from)
    {
        PromotionAttackList[] promotions = new PromotionAttackList[] { _emptyPromotionAttacks, _emptyPromotionAttacks };

        BitBoard board = _whitePawnPatterns[from] & _board.GetBlacks();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            promotions[0] = _whitePromotionAttacks[from][position];
            board = board.Remove(position);

            if (board.Any())
            {
                promotions[1] = _whitePromotionAttacks[from][board.BitScanForward()];
            }
        }

        return promotions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PromotionAttackList[] GetBlackPromotionAttacks(byte from)
    {
        PromotionAttackList[] promotions = new PromotionAttackList[] { _emptyPromotionAttacks, _emptyPromotionAttacks };

        BitBoard board = _blackPawnPatterns[from] & _board.GetWhites();

        if (board.Any())
        {
            byte position = board.BitScanForward();
            promotions[0] = _blackPromotionAttacks[from][position];
            board = board.Remove(position);

            if (board.Any())
            {
                promotions[1] = _blackPromotionAttacks[from][board.BitScanForward()];
            }
        }

        return promotions;
    }

    #endregion
}
