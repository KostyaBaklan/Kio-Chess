using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    private byte _whiteKingPosition;
    private byte _blackKingPosition;

    // Center attack accumulators - computed during piece evaluation
    private int _whiteCenterAttacks;
    private int _blackCenterAttacks;
    //private int _whiteExtendedCenterAttacks;
    //private int _blackExtendedCenterAttacks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        _blackKingPosition = _boards[Pieces.BlackKing].BitScanForward();
        _whiteKingZone = _whiteKingShield[_whiteKingPosition];
        _blackKingZone = _blackKingShield[_blackKingPosition];
        var phase = _moveHistory.GetPhase();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);


        // Initialize center attack accumulators
        _whiteCenterAttacks = (_whitePawnAttacks & _centerSquares).Count();
        _blackCenterAttacks = (_blackPawnAttacks & _centerSquares).Count();
        //_whiteExtendedCenterAttacks = (_whitePawnAttacks & _extendedCenterSquares).Count();
        //_blackExtendedCenterAttacks = (_blackPawnAttacks & _extendedCenterSquares).Count();

        return (phase == Phase.Middle
            ? EvaluateMiddle() : phase == Phase.End
            ? EvaluateEnd() : EvaluateOpening()) + _evaluationService.GetTempoBonus();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EvaluateOpposite()
    {
        _whitePawnAttacks = GetWhitePawnAttacks();
        _blackPawnAttacks = GetBlackPawnAttacks();
        _whiteKingPosition = _boards[Pieces.WhiteKing].BitScanForward();
        _blackKingPosition = _boards[Pieces.BlackKing].BitScanForward();
        _whiteKingZone = _whiteKingShield[_whiteKingPosition];
        _blackKingZone = _blackKingShield[_blackKingPosition];
        var phase = _moveHistory.GetPhase();

        _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);

        // Initialize center attack accumulators
        _whiteCenterAttacks = (_whitePawnAttacks & _centerSquares).Count();
        _blackCenterAttacks = (_blackPawnAttacks & _centerSquares).Count();
        //_whiteExtendedCenterAttacks = (_whitePawnAttacks & _extendedCenterSquares).Count();
        //_blackExtendedCenterAttacks = (_blackPawnAttacks & _extendedCenterSquares).Count();

        return (phase == Phase.Middle
            ? EvaluateMiddleOpposite() : phase == Phase.End
            ? EvaluateEndOpposite() : EvaluateOpeningOpposite()) - _evaluationService.GetTempoBonus();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateEndOpposite() => EvaluateBlackEnd() - EvaluateWhiteEnd() + EvaluateOpposition() - EvaluatePawnMajoritiesEndgame();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateMiddleOpposite() => EvaluateBlackMiddle() - EvaluateWhiteMiddle() - EvaluateCenterControl() - EvaluateDevelopment(); // PHASE 1.6: Skip pawn majorities in middle game (only matters in endgame)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpeningOpposite() => EvaluateBlackOpening() - EvaluateWhiteOpening() - EvaluateCenterControl() - EvaluateDevelopment();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateEnd() => EvaluateWhiteEnd() - EvaluateBlackEnd() - EvaluateOpposition() + EvaluatePawnMajoritiesEndgame(); // PHASE 1.5: Skip center control & development in endgame

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateMiddle() => EvaluateWhiteMiddle() - EvaluateBlackMiddle() + EvaluateCenterControl() + EvaluateDevelopment(); // PHASE 1.6: Skip pawn majorities in middle game (only matters in endgame)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpening() => EvaluateWhiteOpening() - EvaluateBlackOpening() + EvaluateCenterControl() + EvaluateDevelopment();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteOpening()
    {
        var value = EvaluateWhitePawnOpening() + EvaluateWhiteKingOpening();

        if (_boards[Pieces.WhiteKnight].Any())
            value += GetWhiteKnightValueOpening();

        if (_boards[Pieces.WhiteBishop].Any())
            value += GetWhiteBishopValueOpening();

        if (_boards[Pieces.WhiteRook].Any())
            value += EvaluateWhiteRookOpening();

        if (_boards[Pieces.WhiteQueen].Any())
            value += EvaluateWhiteQueenOpening();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteMiddle()
    {
        var value = EvaluateWhitePawnMiddle() + EvaluateWhiteKingMiddle();

        if (_boards[Pieces.WhiteKnight].Any())
            value += EvaluateWhiteKnightMiddle();

        if (_boards[Pieces.WhiteBishop].Any())
            value += GetWhiteBishopValueMiddle();

        if (_boards[Pieces.WhiteRook].Any())
            value += EvaluateWhiteRookMiddle();

        if (_boards[Pieces.WhiteQueen].Any())
            value += EvaluateWhiteQueenMiddle();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteEnd()
    {
        var value = EvaluateWhitePawnEnd() + EvaluateWhiteKingEnd();

        if (_boards[Pieces.WhiteKnight].Any())
            value += EvaluateWhiteKnightEnd();

        if (_boards[Pieces.WhiteBishop].Any())
            value += EvaluateWhiteBishopEnd();

        if (_boards[Pieces.WhiteRook].Any())
            value += EvaluateWhiteRookEnd();

        if (_boards[Pieces.WhiteQueen].Any())
            value += EvaluateWhiteQueenEnd();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackOpening()
    {
        var value = EvaluateBlackPawnOpening() + EvaluateBlackKingOpening();

        if (_boards[Pieces.BlackKnight].Any())
            value += GetBlackKnightValueOpening();

        if (_boards[Pieces.BlackBishop].Any())
            value += GetBlackBishopValueOpening();

        if (_boards[Pieces.BlackRook].Any())
            value += EvaluateBlackRookOpening();

        if (_boards[Pieces.BlackQueen].Any())
            value += EvaluateBlackQueenOpening();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackMiddle()
    {
        var value = EvaluateBlackPawnMiddle() + EvaluateBlackKingMiddle();

        if (_boards[Pieces.BlackKnight].Any())
            value += EvaluateBlackKnightMiddle();

        if (_boards[Pieces.BlackBishop].Any())
            value += GetBlackBishopValueMiddle();

        if (_boards[Pieces.BlackRook].Any())
            value += EvaluateBlackRookMiddle();

        if (_boards[Pieces.BlackQueen].Any())
            value += EvaluateBlackQueenMiddle();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackEnd()
    {
        var value = EvaluateBlackPawnEnd() + EvaluateBlackKingEnd();

        if (_boards[Pieces.BlackKnight].Any())
            value += EvaluateBlackKnightEnd();

        if (_boards[Pieces.BlackBishop].Any())
            value += EvaluateBlackBishopEnd();

        if (_boards[Pieces.BlackRook].Any())
            value += EvaluateBlackRookEnd();

        if (_boards[Pieces.BlackQueen].Any())
            value += EvaluateBlackQueenEnd();

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetStaticValue()
    {
        var _phase = _moveHistory.GetPhase();
        _evaluationService = _evaluationServiceFactory.GetEvaluationService(_phase);
        return GetWhiteStaticValue() - GetBlackStaticValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackStaticValue()
    {
        int value = 0;
        for (byte i = 6; i < 11; i++)
        {
            value += _evaluationService.GetPieceValue(i) * _boards[i].Count();
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteStaticValue()
    {
        int value = 0;
        for (byte i = 0; i < 5; i++)
        {
            value += _evaluationService.GetPieceValue(i) * _boards[i].Count();
        }

        return value;
    }

    /// <summary>
    /// Evaluates center control using pre-accumulated attack counts.
    /// Center attacks are accumulated during piece evaluation to avoid redundant iteration.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateCenterControl()
    {
        // Use pre-accumulated center attack counts (computed during piece evaluation)
        // No need to iterate through pieces again - major performance optimization!
        //int value = (_whiteCenterAttacks - _blackCenterAttacks) * _evaluationService.GetCenterAttackValue();
        //value += (_whiteExtendedCenterAttacks - _blackExtendedCenterAttacks) * _evaluationService.GetExtendedCenterAttackValue();

        return (_whiteCenterAttacks - _blackCenterAttacks) * _evaluationService.GetCenterAttackValue();
    }

    /// <summary>
    /// Evaluates piece development in opening and middle game phases.
    /// Penalizes minor pieces (knights and bishops) that remain on the first rank.
    /// Any minor piece on the back rank is considered undeveloped.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateDevelopment()
    {
        // Only evaluate after threshold ply (e.g., ply 10)
        if (_moveHistory.GetPly() < _evaluationService.GetDevelopmentThresholdMove())
            return 0;

        // Count undeveloped white minor pieces (any on first rank)
        BitBoard whiteUndeveloped = (_boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop])
            & _whiteFirstRank;

        // Count undeveloped black minor pieces (any on eighth rank)
        BitBoard blackUndeveloped = (_boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop])
            & _blackFirstRank;

        // Apply penalty differential (negative if white behind, positive if black behind)
        return (blackUndeveloped.Count() - whiteUndeveloped.Count()) * _evaluationService.GetDevelopmentPenalty();
    }

    /// <summary>
    /// Evaluates opposition in king and pawn endgames using pre-computed patterns.
    /// Opposition only matters when there's minimal material (no pieces besides kings and pawns).
    /// Always returns POSITIVE value if opposition exists (caller controls sign with + or -).
    /// Uses pre-computed bitboards for O(1) detection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpposition()
    {
        // Opposition only relevant in king and pawn endgames (no pieces)
        if (_boards[Pieces.WhiteKnight].Any() || _boards[Pieces.BlackKnight].Any() ||
            _boards[Pieces.WhiteBishop].Any() || _boards[Pieces.BlackBishop].Any() ||
            _boards[Pieces.WhiteRook].Any() || _boards[Pieces.BlackRook].Any() ||
            _boards[Pieces.WhiteQueen].Any() || _boards[Pieces.BlackQueen].Any())
        {
            return 0;  // Opposition not relevant with pieces on board
        }

        // Direct opposition: same file, 1 square between
        if (_directOppositionSquares[_whiteKingPosition].IsSet(_blackKingPosition))
            return _evaluationService.GetDirectOppositionBonus();

        // Diagonal opposition: diagonal, 1 square between
        if (_diagonalOppositionSquares[_whiteKingPosition].IsSet(_blackKingPosition))
            return _evaluationService.GetDiagonalOppositionBonus();

        // Distant opposition: same file, 4-6 squares apart
        if (_distantOppositionSquares[_whiteKingPosition].IsSet(_blackKingPosition))
            return _evaluationService.GetDistantOppositionBonus();

        return 0;
    }

    /// <summary>
    /// Checks if white rook is passively defending own pawns (rook on 1st/2nd rank with pawns ahead).
    /// Uses rook attack generation for accurate detection (accounts for blocking pieces).
    /// OPTIMIZED: Single bitboard operation instead of per-pawn iteration!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsDefendingWhitePawns(byte rookSquare)
    {
        // Get white pawns on same file
        BitBoard pawnsOnFile = _rookFiles[rookSquare] & _boards[Pieces.WhitePawn];

        if (pawnsOnFile.IsZero())
            return false;

        // Check if rook attacks any pawn ahead of it (rook defends what it attacks)
        // This automatically accounts for blocking pieces and validates direct defense
        return (rookSquare.RookAttacks(_occupied) & pawnsOnFile).Any();
    }

    /// <summary>
    /// Checks if black rook is passively defending own pawns (rook on 7th/8th rank with pawns ahead).
    /// Uses rook attack generation for accurate detection (accounts for blocking pieces).
    /// OPTIMIZED: Single bitboard operation instead of per-pawn iteration!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsDefendingBlackPawns(byte rookSquare)
    {
        // Get white pawns on same file
        BitBoard pawnsOnFile = _rookFiles[rookSquare] & _boards[Pieces.BlackPawn];

        if (pawnsOnFile.IsZero())
            return false;

        // Check if rook attacks any pawn ahead of it (rook defends what it attacks)
        // This automatically accounts for blocking pieces and validates direct defense
        return (rookSquare.RookAttacks(_occupied) & pawnsOnFile).Any();
    }

    /// <summary>
    /// Evaluates key square control for a single white passed pawn.
    /// Key squares: Critical squares in front of passed pawns that guarantee promotion.
    /// Uses pre-computed bitboards for O(1) lookups.
    /// Called inline during passed pawn evaluation (zero extra iteration cost).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateWhiteKeySquares(byte pawnSquare)
    {
        BitBoard keySquares = _whiteKeySquares[pawnSquare];

        // Check if white king occupies key square
        if (keySquares.IsSet(_whiteKingPosition))
        {
            return _evaluationService.GetKeySquareControlBonus();
        }
        // Check if black king occupies key square (penalty)
        if (keySquares.IsSet(_blackKingPosition))
        {
            return -_evaluationService.GetKeySquareControlBonus();
        }

        // Proximity bonus (closer king to key squares)
        int distanceDiff = CalculateMinDistance(_blackKingPosition, keySquares) - CalculateMinDistance(_whiteKingPosition, keySquares);

        return distanceDiff * _evaluationService.GetKeySquareProximityFactor();
    }

    /// <summary>
    /// Evaluates key square control for a single black passed pawn.
    /// Key squares: Critical squares behind passed pawns (from black's perspective) that guarantee promotion.
    /// Uses pre-computed bitboards for O(1) lookups.
    /// Called inline during passed pawn evaluation (zero extra iteration cost).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateBlackKeySquares(byte pawnSquare)
    {
        BitBoard keySquares = _blackKeySquares[pawnSquare];

        // Check if black king occupies key square
        if (keySquares.IsSet(_blackKingPosition))
        {
            return _evaluationService.GetKeySquareControlBonus();
        }
        // Check if white king occupies key square (penalty for black)
        if (keySquares.IsSet(_whiteKingPosition))
        {
            return -_evaluationService.GetKeySquareControlBonus();
        }

        // Proximity bonus (closer king to key squares)
        int distanceDiff = CalculateMinDistance(_whiteKingPosition, keySquares) - CalculateMinDistance(_blackKingPosition, keySquares);

        return distanceDiff * _evaluationService.GetKeySquareProximityFactor();
    }

    /// <summary>
    /// Calculates minimum Manhattan distance from king to any key square.
    /// Uses pre-computed Manhattan distance table for O(1) lookups.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CalculateMinDistance(byte kingSquare, BitBoard keySquares)
    {
        int minDistance = 8;
        var bits = keySquares;

        while (bits.Any())
        {
            byte keySquare = bits.BitScanForward();
            int distance = _manhattanDistance[kingSquare][keySquare];
            minDistance = Math.Min(minDistance, distance);
            bits = bits.Remove(keySquare);
        }

        return minDistance;
    }
}