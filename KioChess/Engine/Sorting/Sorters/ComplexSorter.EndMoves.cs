using Engine.DataStructures.Moves.Lists;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Helpers;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    // Queenside and Kingside files for pawn majority detection
    private static readonly BitBoard _queensideFiles = new(0x0F0F0F0F0F0F0F0FUL); // Files A-D
    private static readonly BitBoard _kingsideFiles = new(0xF0F0F0F0F0F0F0F0UL); // Files E-H

    // Precomputed endgame pawn structure bitboards
    private static CellBuffer<BitBoard> _whiteConnectedPawnMasks;
    private static CellBuffer<BitBoard> _blackConnectedPawnMasks;
    private static CellBuffer<BitBoard> _whiteProtectionSquares;
    private static CellBuffer<BitBoard> _blackProtectionSquares;
    private static CellBuffer<BitBoard> _whiteBreakthroughZones;
    private static CellBuffer<BitBoard> _blackBreakthroughZones;

    // NEW: Additional precomputed tables for maximum performance
    private static DistanceBuffer _manhattanDistances;              // [64][64] - Manhattan distance between any two squares
    private static DistanceBuffer _chebyshevDistances;             // [64][64] - Chebyshev (king) distance between any two squares  
    private static CellBuffer<BitBoard> _bishopKeySquareMasks;   // [64] - Key squares controlled by bishop from each position
    private static CellBuffer<BitBoard> _pawnFrontSpans;         // [64] - All squares in front of pawn (white perspective)
    private static CellBuffer<BitBoard> _pawnBackSpans;          // [64] - All squares behind pawn (white perspective)
    private static CellBuffer<BitBoard> _kingZones;              // [64] - Extended king safety zones
    private static CellBuffer<BitBoard> _knightOutposts;         // [64] - Strong outpost squares for knights
    private static CellBuffer<byte> _centralizationValues;             // [64] - Precomputed centralization scores

    // Static initialization of precomputed arrays
    static ComplexSorter()
    {
        InitializeEndgamePawnStructures();
        InitializeAdditionalTables();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitializeEndgamePawnStructures()
    {
        _whiteConnectedPawnMasks = new CellBuffer<BitBoard>();
        _blackConnectedPawnMasks = new CellBuffer<BitBoard>();
        _whiteProtectionSquares = new CellBuffer<BitBoard>();
        _blackProtectionSquares = new CellBuffer<BitBoard>();
        _whiteBreakthroughZones = new CellBuffer<BitBoard>();
        _blackBreakthroughZones = new CellBuffer<BitBoard>();

        // Initialize all arrays
        for (byte square = 0; square < 64; square++)
        {
            _whiteConnectedPawnMasks[square] = ComputeWhiteConnectedPawnMask(square);
            _blackConnectedPawnMasks[square] = ComputeBlackConnectedPawnMask(square);
            _whiteProtectionSquares[square] = ComputeWhiteProtectionSquares(square);
            _blackProtectionSquares[square] = ComputeBlackProtectionSquares(square);
            _whiteBreakthroughZones[square] = ComputeWhiteBreakthroughZone(square);
            _blackBreakthroughZones[square] = ComputeBlackBreakthroughZone(square);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitializeAdditionalTables()
    {
        // Initialize distance tables
        _manhattanDistances = new();
        _chebyshevDistances = new();
        for (int i = 0; i < 64; i++)
        {
            _manhattanDistances[i] = new();
            _chebyshevDistances[i] = new();
        }

        // Initialize other arrays
        _bishopKeySquareMasks = new CellBuffer<BitBoard>();
        _pawnFrontSpans = new CellBuffer<BitBoard>();
        _pawnBackSpans = new CellBuffer<BitBoard>();
        _kingZones = new CellBuffer<BitBoard>();
        _knightOutposts = new CellBuffer<BitBoard>();
        _centralizationValues = new CellBuffer<byte>();

        // Compute all precomputed values
        for (byte square = 0; square < 64; square++)
        {
            ComputeDistanceTables(square);
            _bishopKeySquareMasks[square] = ComputeBishopKeySquareMask(square);
            _pawnFrontSpans[square] = ComputePawnFrontSpan(square);
            _pawnBackSpans[square] = ComputePawnBackSpan(square);
            _kingZones[square] = ComputeKingZone(square);
            _knightOutposts[square] = ComputeKnightOutpost(square);
            _centralizationValues[square] = ComputeCentralizationValue(square);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeDistanceTables(byte square)
    {
        var file1 = square % 8;
        var rank1 = square / 8;

        for (byte target = 0; target < 64; target++)
        {
            var file2 = target % 8;
            var rank2 = target / 8;

            // Manhattan distance (taxicab distance)
            _manhattanDistances[square][target] = (byte)(Math.Abs(file1 - file2) + Math.Abs(rank1 - rank2));

            // Chebyshev distance (king distance - max of file/rank difference)
            _chebyshevDistances[square][target] = (byte)Math.Max(Math.Abs(file1 - file2), Math.Abs(rank1 - rank2));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeBishopKeySquareMask(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Diagonal key squares - important for bishop control evaluation
        // Add squares on both diagonals extending from bishop position
        for (int d = 1; d < 8; d++)
        {
            // Up-right diagonal
            if (file + d < 8 && rank + d < 8)
            {
                var keySquare = (byte)((rank + d) * 8 + (file + d));
                mask |= keySquare.AsBitBoard();
            }

            // Up-left diagonal  
            if (file - d >= 0 && rank + d < 8)
            {
                var keySquare = (byte)((rank + d) * 8 + (file - d));
                mask |= keySquare.AsBitBoard();
            }

            // Down-right diagonal
            if (file + d < 8 && rank - d >= 0)
            {
                var keySquare = (byte)((rank - d) * 8 + (file + d));
                mask |= keySquare.AsBitBoard();
            }

            // Down-left diagonal
            if (file - d >= 0 && rank - d >= 0)
            {
                var keySquare = (byte)((rank - d) * 8 + (file - d));
                mask |= keySquare.AsBitBoard();
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputePawnFrontSpan(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // All squares in front of the pawn (white perspective)
        for (int r = rank + 1; r < 8; r++)
        {
            var frontSquare = (byte)(r * 8 + file);
            mask |= frontSquare.AsBitBoard();
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputePawnBackSpan(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // All squares behind the pawn (white perspective)
        for (int r = rank - 1; r >= 0; r--)
        {
            var backSquare = (byte)(r * 8 + file);
            mask |= backSquare.AsBitBoard();
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeKingZone(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Extended king zone (2-square radius)
        for (int fileOffset = -2; fileOffset <= 2; fileOffset++)
        {
            for (int rankOffset = -2; rankOffset <= 2; rankOffset++)
            {
                var targetFile = file + fileOffset;
                var targetRank = rank + rankOffset;

                if (targetFile >= 0 && targetFile < 8 && targetRank >= 0 && targetRank < 8)
                {
                    var zoneSquare = (byte)(targetRank * 8 + targetFile);
                    mask |= zoneSquare.AsBitBoard();
                }
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeKnightOutpost(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Strong outpost squares for knights (center-biased, protected squares)
        if (rank >= 3 && rank <= 5 && file >= 2 && file <= 5)
        {
            // This is a strong outpost area
            mask |= square.AsBitBoard();

            // Add adjacent protection squares
            if (rank > 0)
            {
                if (file > 0)
                {
                    var protectSquare = (byte)((rank - 1) * 8 + (file - 1));
                    mask |= protectSquare.AsBitBoard();
                }
                if (file < 7)
                {
                    var protectSquare = (byte)((rank - 1) * 8 + (file + 1));
                    mask |= protectSquare.AsBitBoard();
                }
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ComputeCentralizationValue(byte square)
    {
        var file = square % 8;
        var rank = square / 8;

        // Calculate distance from center (files D/E, ranks 4/5)
        var fileCenterDistance = Math.Min(Math.Abs(file - 3), Math.Abs(file - 4));
        var rankCenterDistance = Math.Min(Math.Abs(rank - 3), Math.Abs(rank - 4));
        var totalDistance = fileCenterDistance + rankCenterDistance;

        // Convert distance to centralization value (higher = more central)
        return (byte)(8 - totalDistance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeWhiteConnectedPawnMask(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Adjacent files for connected pawns (same rank or within 1 rank)
        for (int rankOffset = -1; rankOffset <= 1; rankOffset++)
        {
            var targetRank = rank + rankOffset;
            if (targetRank < 0 || targetRank > 7) continue;

            // Left adjacent file
            if (file > 0)
            {
                var leftSquare = (byte)(targetRank * 8 + (file - 1));
                mask |= leftSquare.AsBitBoard();
            }

            // Right adjacent file
            if (file < 7)
            {
                var rightSquare = (byte)(targetRank * 8 + (file + 1));
                mask |= rightSquare.AsBitBoard();
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeBlackConnectedPawnMask(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Adjacent files for connected pawns (same rank or within 1 rank)
        for (int rankOffset = -1; rankOffset <= 1; rankOffset++)
        {
            var targetRank = rank + rankOffset;
            if (targetRank < 0 || targetRank > 7) continue;

            // Left adjacent file
            if (file > 0)
            {
                var leftSquare = (byte)(targetRank * 8 + (file - 1));
                mask |= leftSquare.AsBitBoard();
            }

            // Right adjacent file
            if (file < 7)
            {
                var rightSquare = (byte)(targetRank * 8 + (file + 1));
                mask |= rightSquare.AsBitBoard();
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeWhiteProtectionSquares(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Diagonal protection squares behind the pawn
        if (rank > 0)
        {
            // Left diagonal behind
            if (file > 0)
            {
                var protectorSquare = (byte)((rank - 1) * 8 + (file - 1));
                mask |= protectorSquare.AsBitBoard();
            }

            // Right diagonal behind
            if (file < 7)
            {
                var protectorSquare = (byte)((rank - 1) * 8 + (file + 1));
                mask |= protectorSquare.AsBitBoard();
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeBlackProtectionSquares(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Diagonal protection squares behind the pawn (for black, that's higher ranks)
        if (rank < 7)
        {
            // Left diagonal behind
            if (file > 0)
            {
                var protectorSquare = (byte)((rank + 1) * 8 + (file - 1));
                mask |= protectorSquare.AsBitBoard();
            }

            // Right diagonal behind
            if (file < 7)
            {
                var protectorSquare = (byte)((rank + 1) * 8 + (file + 1));
                mask |= protectorSquare.AsBitBoard();
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeWhiteBreakthroughZone(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Path to promotion for white (ranks above current position)
        for (int r = rank + 1; r < 8; r++)
        {
            // Direct path
            var directSquare = (byte)(r * 8 + file);
            mask |= directSquare.AsBitBoard();

            // Diagonal capture squares
            if (file > 0)
            {
                var leftDiagonalSquare = (byte)(r * 8 + (file - 1));
                mask |= leftDiagonalSquare.AsBitBoard();
            }

            if (file < 7)
            {
                var rightDiagonalSquare = (byte)(r * 8 + (file + 1));
                mask |= rightDiagonalSquare.AsBitBoard();
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard ComputeBlackBreakthroughZone(byte square)
    {
        var file = square % 8;
        var rank = square / 8;
        BitBoard mask = new();

        // Path to promotion for black (ranks below current position)
        for (int r = rank - 1; r >= 0; r--)
        {
            // Direct path
            var directSquare = (byte)(r * 8 + file);
            mask |= directSquare.AsBitBoard();

            // Diagonal capture squares
            if (file > 0)
            {
                var leftDiagonalSquare = (byte)(r * 8 + (file - 1));
                mask |= leftDiagonalSquare.AsBitBoard();
            }

            if (file < 7)
            {
                var rightDiagonalSquare = (byte)(r * 8 + (file + 1));
                mask |= rightDiagonalSquare.AsBitBoard();
            }
        }

        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessWhiteEndMove(MoveBase move)
    {
        Position.MakeWhite(move);

        bool hasResult = CheckWhiteResult(move);

        Position.UnMakeWhite();

        if (hasResult)
            return;

        switch (move.Piece)
        {
            case Pieces.WhitePawn:
                ProcessWhitePawnEndMove(move);
                break;
            case Pieces.WhiteRook:
                ProcessWhiteRookEndMove(move);
                break;
            case Pieces.WhiteQueen:
                ProcessWhiteQueenEndMove(move);
                break;
            case Pieces.WhiteKing:
                ProcessWhiteKingEndMove(move);
                break;
            case Pieces.WhiteKnight:
                ProcessWhiteKnightEndMove(move);
                break;
            case Pieces.WhiteBishop:
                ProcessWhiteBishopEndMove(move);
                break;
            default:
                AttackCollection.AddNonCapture(move);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessBlackEndMove(MoveBase move)
    {
        Position.MakeBlack(move);
        bool hasResult = CheckBlackResult(move);

        Position.UnMakeBlack();

        if (hasResult)
            return;

        switch (move.Piece)
        {
            case Pieces.BlackPawn:
                ProcessBlackPawnEndMove(move);
                break;
            case Pieces.BlackRook:
                ProcessBlackRookEndMove(move);
                break;
            case Pieces.BlackQueen:
                ProcessBlackQueenEndMove(move);
                break;
            case Pieces.BlackKing:
                ProcessBlackKingEndMove(move);
                break;
            case Pieces.BlackKnight:
                ProcessBlackKnightEndMove(move);
                break;
            case Pieces.BlackBishop:
                ProcessBlackBishopEndMove(move);
                break;
            default:
                AttackCollection.AddNonCapture(move);
                break;
        }
    }

    // Enhanced White pawn endgame move processing with precomputed arrays
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhitePawnEndMove(MoveBase move)
    {
        if (Board.IsWhitePass(move.To))
        {
            // Use precomputed arrays for maximum performance
            var pawnBoard = Board.GetPieceBits(Pieces.WhitePawn);
            var isConnected = (_whiteConnectedPawnMasks[move.To] & pawnBoard).Any();
            var isProtected = (_whiteProtectionSquares[move.To] & pawnBoard).Any();

            // Highest priority: Connected and protected passed pawns
            if (isConnected || isProtected || IsWhiteOutsidePassedPawn(move.To))
            {
                AttackCollection.AddSuggested(move);
            }
            // Medium priority: Regular passed pawns
            else
            {
                AttackCollection.AddForwardMove(move);
            }
        }
        else if (Board.IsWhiteCandidate(move.From, move.To) || IsWhitePawnBreakthrough(move))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    // Enhanced Black pawn endgame move processing with precomputed arrays
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackPawnEndMove(MoveBase move)
    {
        if (Board.IsBlackPass(move.To))
        {
            var pawnBoard = Board.GetPieceBits(Pieces.BlackPawn);
            var isConnected = (_blackConnectedPawnMasks[move.To] & pawnBoard).Any();
            var isProtected = (_blackProtectionSquares[move.To] & pawnBoard).Any();

            if (isConnected || isProtected || IsBlackOutsidePassedPawn(move.To))
            {
                AttackCollection.AddSuggested(move);
            }
            else
            {
                AttackCollection.AddForwardMove(move);
            }
        }
        else if (Board.IsBlackCandidate(move.From, move.To) || IsBlackPawnBreakthrough(move))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteRookEndMove(MoveBase move)
    {
        if (Board.IsBehindWhitePassed(move.From, move.To) ||
            Board.IsWhiteRookAttacksKingZone(move.From, move.To) ||
            IsWhiteRookCuttingOffEnemyKing(move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteQueenEndMove(MoveBase move)
    {
        if (Board.IsWhiteQueenAttacksKingZone(move.From, move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteKingEndMove(MoveBase move)
    {
        // King activity is crucial in endgames - use precomputed tables
        if (IsWhiteKingImprovedActivity(move.From, move.To))
        {
            AttackCollection.AddSuggested(move);
        }
        else if (IsWhiteKingSupportingPawns(move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteKnightEndMove(MoveBase move)
    {
        if (IsKnightImprovedEndgamePosition(move.From, move.To) ||
            IsWhiteKnightBlockingEnemyPawns(move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessWhiteBishopEndMove(MoveBase move)
    {
        if (IsBishopControllingKeySquaresFast(move.To, Board.GetPieceBits(Pieces.BlackPawn)))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackRookEndMove(MoveBase move)
    {
        if (Board.IsBehindBlackPassed(move.From, move.To) ||
            Board.IsBlackRookAttacksKingZone(move.From, move.To) ||
            IsBlackRookCuttingOffEnemyKing(move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackQueenEndMove(MoveBase move)
    {
        if (Board.IsBlackQueenAttacksKingZone(move.From, move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackKingEndMove(MoveBase move)
    {
        if (IsBlackKingImprovedActivity(move.From, move.To))
        {
            AttackCollection.AddSuggested(move);
        }
        else if (IsBlackKingSupportingPawns(move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackKnightEndMove(MoveBase move)
    {
        if (IsKnightImprovedEndgamePosition(move.From, move.To) ||
            IsBlackKnightBlockingEnemyPawns(move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessBlackBishopEndMove(MoveBase move)
    {
        if (IsBishopControllingKeySquaresFast(move.To, Board.GetPieceBits(Pieces.WhitePawn)))
        {
            AttackCollection.AddForwardMove(move);
        }
        else
        {
            AttackCollection.AddNonCapture(move);
        }
    }

    // High-performance pawn structure detection using precomputed arrays

    // Outside passed pawn detection (optimized)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteOutsidePassedPawn(byte square)
    {
        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);

        if (blackPawns.IsZero()) return true;

        // Quick file-based check using bitboard operations
        return square % 8 < 4
            ? (blackPawns & _kingsideFiles).Any() && (blackPawns & _queensideFiles).IsZero()
            : (blackPawns & _queensideFiles).Any() && (blackPawns & _kingsideFiles).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackOutsidePassedPawn(byte square)
    {
        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);

        if (whitePawns.IsZero()) return true;

        return square % 8 < 4
            ? (whitePawns & _kingsideFiles).Any() && (whitePawns & _queensideFiles).IsZero()
            : (whitePawns & _queensideFiles).Any() && (whitePawns & _kingsideFiles).IsZero();
    }

    // Pawn breakthrough detection using precomputed zones
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhitePawnBreakthrough(MoveBase move)
    {
        return move.To / 8 >= 5 && (_whiteBreakthroughZones[move.To] & Board.GetPieceBits(Pieces.BlackPawn)).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackPawnBreakthrough(MoveBase move)
    {
        return move.To / 8 <= 2 && (_blackBreakthroughZones[move.To] & Board.GetPieceBits(Pieces.WhitePawn)).IsZero();
    }

    // OPTIMIZED: King activity evaluation using precomputed distance tables
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteKingImprovedActivity(byte from, byte to) => _centralizationValues[to] > _centralizationValues[from] || IsGainingOppositionFast(to, Board.GetBlackKingPosition(), from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackKingImprovedActivity(byte from, byte to) => _centralizationValues[to] > _centralizationValues[from] || IsGainingOppositionFast(to, Board.GetWhiteKingPosition(), from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteKingSupportingPawns(byte square)
    {
        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);
        return whitePawns.Any() && GetDistanceToNearestPawnFast(square, whitePawns) <= 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackKingSupportingPawns(byte square)
    {
        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);
        return blackPawns.Any() && GetDistanceToNearestPawnFast(square, blackPawns) <= 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBishopControllingKeySquaresFast(byte bishopSquare, BitBoard enemyPawns)
    {
        // Use precomputed bishop key squares for instant collision detection
        var keySquares = _bishopKeySquareMasks[bishopSquare];

        // Check if any enemy pawn's front span intersects with bishop's key squares
        while (enemyPawns.Any())
        {
            var pawn = enemyPawns.BitScanForward();
            var pawnFrontSpan = _pawnFrontSpans[pawn];

            if ((keySquares & pawnFrontSpan).Any()) return true;

            enemyPawns = enemyPawns.Remove(pawn);
        }

        return false;
    }

    // OPTIMIZED: Knight endgame positioning using precomputed outpost evaluation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsKnightImprovedEndgamePosition(byte from, byte to)
    {
        return _centralizationValues[to] > _centralizationValues[from] || (_knightOutposts[to] & to.AsBitBoard()).Any() && (_knightOutposts[from] & from.AsBitBoard()).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteKnightBlockingEnemyPawns(byte square) => GetDistanceToNearestPawnFast(square, Board.GetPieceBits(Pieces.BlackPawn)) <= 2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackKnightBlockingEnemyPawns(byte square) => GetDistanceToNearestPawnFast(square, Board.GetPieceBits(Pieces.WhitePawn)) <= 2;

    // OPTIMIZED: Rook endgame activity using precomputed king zones
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteRookCuttingOffEnemyKing(byte square) =>
        // Use precomputed distance table for instant distance check
        (square.RookAttacks(Board.GetOccupied()) & Board.GetPieceBits(Pieces.BlackKing)).IsZero() && _manhattanDistances[square][Board.GetBlackKingPosition()] <= 4;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackRookCuttingOffEnemyKing(byte square) => (square.RookAttacks(Board.GetOccupied()) & Board.GetPieceBits(Pieces.WhiteKing)).IsZero() && _manhattanDistances[square][Board.GetWhiteKingPosition()] <= 4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetDistanceToNearestPawnFast(byte square, BitBoard pawns)
    {
        if (pawns.IsZero()) return 8;

        int minDistance = 8;
        while (pawns.Any())
        {
            var pawn = pawns.BitScanForward();
            var distance = _manhattanDistances[square][pawn];
            if (distance < minDistance) minDistance = distance;
            pawns = pawns.Remove(pawn);
        }

        return minDistance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsGainingOppositionFast(byte kingTo, byte enemyKing, byte kingFrom) => _chebyshevDistances[kingTo][enemyKing] == 2 && _chebyshevDistances[kingFrom][enemyKing] != 2;
}