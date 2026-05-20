using Engine.Models.Bits;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    #region Total Mobility

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalBlackQueenMobility(byte from) => (from.QueenAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalBlackRookMobility(byte from) => (from.RookAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalBlackBishopMobility(byte from) => (from.BishopAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalBlackKnightMobility(byte from) => (_blackKnightPatterns[from] & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalWhiteQueenMobility(byte from) => (from.QueenAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalWhiteRookMobility(byte from) => (from.RookAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalWhiteBishopMobility(byte from) => (from.BishopAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountTotalWhiteKnightMobility(byte from) => (_whiteKnightPatterns[from] & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalBlackQueenMobility(byte from) => CountTotalBlackQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalBlackRookMobility(byte from) => CountTotalBlackRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalBlackBishopMobility(byte from) => CountTotalBlackBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalBlackKnightMobility(byte from) => CountTotalBlackKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalWhiteQueenMobility(byte from) => CountTotalWhiteQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalWhiteRookMobility(byte from) => CountTotalWhiteRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalWhiteBishopMobility(byte from) => CountTotalWhiteBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTotalWhiteKnightMobility(byte from) => CountTotalWhiteKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CountTotalBlackMobility()
    {
        int totalMobility = 0;
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.BlackKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalBlackKnightMobility(from);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalBlackBishopMobility(from);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalBlackRookMobility(from);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalBlackQueenMobility(from);
            board = board.Remove(from);
        }

        return totalMobility;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CountTotalWhiteMobility()
    {
        int totalMobility = 0;
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalWhiteKnightMobility(from);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalWhiteBishopMobility(from);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalWhiteRookMobility(from);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            totalMobility += CountTotalWhiteQueenMobility(from);
            board = board.Remove(from);
        }

        return totalMobility;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TotalWhiteMobility()
    {
        return CountTotalWhiteMobility() - CountTotalBlackMobility();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TotalBlackMobility()
    {
        return CountTotalBlackMobility() - CountTotalWhiteMobility();
    }

    #endregion

    #region Relative Mobility

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeBlackQueenMobility(byte from) => _blackQueenPatterns[from].Count() - (from.QueenAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeBlackRookMobility(byte from) => (from.RookAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeBlackBishopMobility(byte from) => (from.BishopAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeBlackKnightMobility(byte from) => (_blackKnightPatterns[from] & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeWhiteQueenMobility(byte from) => (from.QueenAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeWhiteRookMobility(byte from) => (from.RookAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeWhiteBishopMobility(byte from) => (from.BishopAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeWhiteKnightMobility(byte from) => (_whiteKnightPatterns[from] & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeBlackQueenMobility(byte from) => CountRelativeBlackQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeBlackRookMobility(byte from) => CountRelativeBlackRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeBlackBishopMobility(byte from) => CountRelativeBlackBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeBlackKnightMobility(byte from) => CountRelativeBlackKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeWhiteQueenMobility(byte from) => CountRelativeWhiteQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeWhiteRookMobility(byte from) => CountRelativeWhiteRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeWhiteBishopMobility(byte from) => CountRelativeWhiteBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRelativeWhiteKnightMobility(byte from) => CountRelativeWhiteKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeBlackMobility()
    {
        BitBoard SafeMobility = new BitBoard();
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.BlackKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= _blackKnightPatterns[from];
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.BishopAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.RookAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.QueenAttacks(_occupied);
            board = board.Remove(from);
        }

        return (SafeMobility & (_empty | _whites)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountRelativeWhiteMobility()
    {
        BitBoard SafeMobility = new BitBoard();
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= _whiteKnightPatterns[from];
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.BishopAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.RookAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.QueenAttacks(_occupied);
            board = board.Remove(from);
        }

        return (SafeMobility & (_empty | _blacks)).Count();
    }

    #endregion

    #region Safe Mobility

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeBlackQueenMobility(byte from) => (from.QueenAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeBlackRookMobility(byte from) => (from.RookAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeBlackBishopMobility(byte from) => (from.BishopAttacks(_occupied) & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeBlackKnightMobility(byte from) => (_blackKnightPatterns[from] & (_empty | _whites)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeWhiteQueenMobility(byte from) => (from.QueenAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeWhiteRookMobility(byte from) => (from.RookAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeWhiteBishopMobility(byte from) => (from.BishopAttacks(_occupied) & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeWhiteKnightMobility(byte from) => (_whiteKnightPatterns[from] & (_empty | _blacks)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeBlackQueenMobility(byte from) => CountSafeBlackQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeBlackRookMobility(byte from) => CountSafeBlackRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeBlackBishopMobility(byte from) => CountSafeBlackBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeBlackKnightMobility(byte from) => CountSafeBlackKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeWhiteQueenMobility(byte from) => CountSafeWhiteQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeWhiteRookMobility(byte from) => CountSafeWhiteRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeWhiteBishopMobility(byte from) => CountSafeWhiteBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSafeWhiteKnightMobility(byte from) => CountSafeWhiteKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeBlackMobility()
    {
        BitBoard SafeMobility = new BitBoard();
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.BlackKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= _blackKnightPatterns[from];
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.BishopAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.RookAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.QueenAttacks(_occupied);
            board = board.Remove(from);
        }

        return (SafeMobility & (_empty | _whites)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountSafeWhiteMobility()
    {
        BitBoard SafeMobility = new BitBoard();
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= _whiteKnightPatterns[from];
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.BishopAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.RookAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            SafeMobility |= from.QueenAttacks(_occupied);
            board = board.Remove(from);
        }

        return (SafeMobility & (_empty | _blacks)).Count();
    }

    #endregion

    #region Evaluation Mobility

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationBlackQueenMobility(byte from) =>
        (_blackQueenAttacks[from] & (_empty.Remove(_whitePawnAttacks) | _whiteKingZone)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationBlackRookMobility(byte from) =>
        (_blackRookAttacks[from] & (_empty.Remove(_whitePawnAttacks) | _whiteKingZone)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationBlackBishopMobility(byte from)
    {
        ref var boardBase = ref _boards[0];
        return (_blackBishopAttacks[from] & (_empty.Remove(_whitePawnAttacks) | _whiteKingZone | Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteKnight))).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationBlackKnightMobility(byte from)
    {
        ref var boardBase = ref _boards[0];
        return (_blackKnightPatterns[from] & (_empty.Remove(_whitePawnAttacks) | _whiteKingZone | Unsafe.Add(ref boardBase, Pieces.WhiteQueen) | Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop))).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationWhiteQueenMobility(byte from) =>
        (_whiteQueenAttacks[from] & (_empty.Remove(_blackPawnAttacks) | _blackKingZone)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationWhiteRookMobility(byte from) =>
        (_whiteRookAttacks[from] & (_empty.Remove(_blackPawnAttacks) | _blackKingZone)).Count();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationWhiteBishopMobility(byte from)
    {
        ref var boardBase = ref _boards[0];
        return (_whiteBishopAttacks[from] & (_empty.Remove(_blackPawnAttacks) | _blackKingZone | Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackKnight))).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationWhiteKnightMobility(byte from)
    {
        ref var boardBase = ref _boards[0];
        return (_whiteKnightPatterns[from] & (_empty.Remove(_blackPawnAttacks) | _blackKingZone | Unsafe.Add(ref boardBase, Pieces.BlackQueen) | Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackBishop))).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationBlackQueenMobility(byte from) => CountEvaluationBlackQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationBlackRookMobility(byte from) => CountEvaluationBlackRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationBlackBishopMobility(byte from) => CountEvaluationBlackBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationBlackKnightMobility(byte from) => CountEvaluationBlackKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationWhiteQueenMobility(byte from) => CountEvaluationWhiteQueenMobility(from)
        * _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationWhiteRookMobility(byte from) => CountEvaluationWhiteRookMobility(from)
        * _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationWhiteBishopMobility(byte from) => CountEvaluationWhiteBishopMobility(from)
        * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEvaluationWhiteKnightMobility(byte from) => CountEvaluationWhiteKnightMobility(from)
        * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationBlackMobility()
    {
        BitBoard EvaluationMobility = new BitBoard();
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.BlackKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= _blackKnightPatterns[from];
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= from.BishopAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= from.RookAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= from.QueenAttacks(_occupied);
            board = board.Remove(from);
        }

        return (EvaluationMobility & (_empty | _whites)).Count();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CountEvaluationWhiteMobility()
    {
        BitBoard EvaluationMobility = new BitBoard();
        ref var boardBase = ref _boards[0];

        var board = Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= _whiteKnightPatterns[from];
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteBishop);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= from.BishopAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteRook);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= from.RookAttacks(_occupied);
            board = board.Remove(from);
        }

        board = Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        while (board.Any())
        {
            var from = board.BitScanForward();
            EvaluationMobility |= from.QueenAttacks(_occupied);
            board = board.Remove(from);
        }

        return (EvaluationMobility & (_empty | _blacks)).Count();
    }

    #endregion
}