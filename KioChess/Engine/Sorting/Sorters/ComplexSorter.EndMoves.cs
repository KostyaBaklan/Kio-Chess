using Engine.DataStructures.Moves.Lists;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Helpers;
using Engine.Models.Boards;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter
{
    // Center squares for king activity evaluation
    private static readonly BitBoard _centerSquares = Squares.D4.AsBitBoard() | Squares.D5.AsBitBoard() |
                                                     Squares.E4.AsBitBoard() | Squares.E5.AsBitBoard();
    private static readonly BitBoard _extendedCenter = Squares.C3.AsBitBoard() | Squares.C4.AsBitBoard() | Squares.C5.AsBitBoard() | Squares.C6.AsBitBoard() |
                                                      Squares.D3.AsBitBoard() | Squares.D4.AsBitBoard() | Squares.D5.AsBitBoard() | Squares.D6.AsBitBoard() |
                                                      Squares.E3.AsBitBoard() | Squares.E4.AsBitBoard() | Squares.E5.AsBitBoard() | Squares.E6.AsBitBoard() |
                                                      Squares.F3.AsBitBoard() | Squares.F4.AsBitBoard() | Squares.F5.AsBitBoard() | Squares.F6.AsBitBoard();

    // Queenside and Kingside files for pawn majority detection
    private static readonly BitBoard _queensideFiles = new(0x0F0F0F0F0F0F0F0FUL); // Files A-D
    private static readonly BitBoard _kingsideFiles = new(0xF0F0F0F0F0F0F0F0UL); // Files E-H

    // Precomputed endgame pawn structure bitboards
    private static BitBoard[] _whiteConnectedPawnMasks;
    private static BitBoard[] _blackConnectedPawnMasks;
    private static BitBoard[] _whiteProtectionSquares;
    private static BitBoard[] _blackProtectionSquares;
    private static BitBoard[] _whiteBreakthroughZones;
    private static BitBoard[] _blackBreakthroughZones;

    // NEW: Additional precomputed tables for maximum performance
    private static byte[][] _manhattanDistances;              // [64][64] - Manhattan distance between any two squares
    private static byte[][] _chebyshevDistances;             // [64][64] - Chebyshev (king) distance between any two squares  
    private static BitBoard[] _bishopKeySquareMasks;   // [64] - Key squares controlled by bishop from each position
    private static BitBoard[] _pawnFrontSpans;         // [64] - All squares in front of pawn (white perspective)
    private static BitBoard[] _pawnBackSpans;          // [64] - All squares behind pawn (white perspective)
    private static BitBoard[] _kingZones;              // [64] - Extended king safety zones
    private static BitBoard[] _knightOutposts;         // [64] - Strong outpost squares for knights
    private static byte[] _centralizationValues;             // [64] - Precomputed centralization scores

    // Static initialization of precomputed arrays
    static ComplexSorter()
    {
        InitializeEndgamePawnStructures();
        InitializeAdditionalTables();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitializeEndgamePawnStructures()
    {
        _whiteConnectedPawnMasks = new BitBoard[64];
        _blackConnectedPawnMasks = new BitBoard[64];
        _whiteProtectionSquares = new BitBoard[64];
        _blackProtectionSquares = new BitBoard[64];
        _whiteBreakthroughZones = new BitBoard[64];
        _blackBreakthroughZones = new BitBoard[64];

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
        _manhattanDistances = new byte[64][];
        _chebyshevDistances = new byte[64][];
        for (int i = 0; i < 64; i++)
        {
            _manhattanDistances[i] = new byte[64];
            _chebyshevDistances[i] = new byte[64];
        }

        // Initialize other arrays
        _bishopKeySquareMasks = new BitBoard[64];
        _pawnFrontSpans = new BitBoard[64];
        _pawnBackSpans = new BitBoard[64];
        _kingZones = new BitBoard[64];
        _knightOutposts = new BitBoard[64];
        _centralizationValues = new byte[64];

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
            if (isConnected && isProtected)
            {
                AttackCollection.AddSuggested(move);
            }
            // High priority: Outside passed pawns or connected passed pawns
            else if (IsWhiteOutsidePassedPawn(move.To) || isConnected)
            {
                AttackCollection.AddSuggested(move);
            }
            // Medium-high priority: Protected passed pawns
            else if (isProtected)
            {
                AttackCollection.AddForwardMove(move);
            }
            // Medium priority: Regular passed pawns
            else
            {
                AttackCollection.AddForwardMove(move);
            }
        }
        else if (Board.IsWhiteCandidate(move.From, move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        // Check for pawn breakthrough patterns using precomputed zones
        else if (IsWhitePawnBreakthrough(move))
        {
            AttackCollection.AddForwardMove(move);
        }
        // Check for creating pawn majority
        else if (IsImprovingWhitePawnMajority(move))
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

            if (isConnected && isProtected)
            {
                AttackCollection.AddSuggested(move);
            }
            else if (IsBlackOutsidePassedPawn(move.To) || isConnected)
            {
                AttackCollection.AddSuggested(move);
            }
            else if (isProtected)
            {
                AttackCollection.AddForwardMove(move);
            }
            else
            {
                AttackCollection.AddForwardMove(move);
            }
        }
        else if (Board.IsBlackCandidate(move.From, move.To))
        {
            AttackCollection.AddForwardMove(move);
        }
        else if (IsBlackPawnBreakthrough(move))
        {
            AttackCollection.AddForwardMove(move);
        }
        else if (IsImprovingBlackPawnMajority(move))
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
            IsRookCuttingOffEnemyKing(move.To, false))
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
        if (Board.IsWhiteQueenAttacksKingZone(move.From, move.To) ||
            IsQueenImprovedActivity(move.From, move.To))
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
            IsKnightBlockingEnemyPawns(move.To, false))
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
        if (IsWhiteBishopImprovedEndgame(move.From, move.To))
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
            IsRookCuttingOffEnemyKing(move.To, true))
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
        if (Board.IsBlackQueenAttacksKingZone(move.From, move.To) ||
            IsQueenImprovedActivity(move.From, move.To))
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
            IsKnightBlockingEnemyPawns(move.To, true))
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
        if (IsBlackBishopImprovedEndgame(move.From, move.To))
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
        if (!Board.IsWhitePass(square)) return false;

        var file = square % 8;
        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);

        if (blackPawns.IsZero()) return true;

        // Quick file-based check using bitboard operations
        if (file <= 3) // Queenside
        {
            return (blackPawns & _kingsideFiles).Any() && (blackPawns & _queensideFiles).IsZero();
        }
        else // Kingside
        {
            return (blackPawns & _queensideFiles).Any() && (blackPawns & _kingsideFiles).IsZero();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackOutsidePassedPawn(byte square)
    {
        if (!Board.IsBlackPass(square)) return false;

        var file = square % 8;
        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);

        if (whitePawns.IsZero()) return true;

        if (file <= 3)
        {
            return (whitePawns & _kingsideFiles).Any() && (whitePawns & _queensideFiles).IsZero();
        }
        else
        {
            return (whitePawns & _queensideFiles).Any() && (whitePawns & _kingsideFiles).IsZero();
        }
    }

    // Pawn breakthrough detection using precomputed zones
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhitePawnBreakthrough(MoveBase move)
    {
        var rank = move.To / 8;

        // Check if moving to 6th rank or beyond
        if (rank < 5) return false;

        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);

        // Use precomputed breakthrough zone for ultra-fast collision detection
        return (_whiteBreakthroughZones[move.To] & blackPawns).IsZero();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackPawnBreakthrough(MoveBase move)
    {
        var rank = move.To / 8;

        if (rank > 2) return false;

        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);

        return (_blackBreakthroughZones[move.To] & whitePawns).IsZero();
    }

    // Pawn majority improvement detection (optimized)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsImprovingWhitePawnMajority(MoveBase move)
    {
        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);
        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);

        // Fast bitboard-based pawn counting
        var whiteQueenside = (whitePawns & _queensideFiles).Count();
        var whiteKingside = (whitePawns & _kingsideFiles).Count();
        var blackQueenside = (blackPawns & _queensideFiles).Count();
        var blackKingside = (blackPawns & _kingsideFiles).Count();

        var moveFile = move.To % 8;
        var isQueensideMove = moveFile < 4;

        if (isQueensideMove)
        {
            return whiteQueenside > blackQueenside && whiteKingside <= blackKingside;
        }
        else
        {
            return whiteKingside > blackKingside && whiteQueenside <= blackQueenside;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsImprovingBlackPawnMajority(MoveBase move)
    {
        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);
        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);

        var whiteQueenside = (whitePawns & _queensideFiles).Count();
        var whiteKingside = (whitePawns & _kingsideFiles).Count();
        var blackQueenside = (blackPawns & _queensideFiles).Count();
        var blackKingside = (blackPawns & _kingsideFiles).Count();

        var moveFile = move.To % 8;
        var isQueensideMove = moveFile < 4;

        if (isQueensideMove)
        {
            return blackQueenside > whiteQueenside && blackKingside <= whiteKingside;
        }
        else
        {
            return blackKingside > whiteKingside && blackQueenside <= whiteQueenside;
        }
    }

    // OPTIMIZED: King activity evaluation using precomputed distance tables
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteKingImprovedActivity(byte from, byte to)
    {
        // Use precomputed centralization values
        var fromCentralization = _centralizationValues[from];
        var toCentralization = _centralizationValues[to];

        if (toCentralization > fromCentralization) return true;

        // Opposition check using precomputed distances
        var blackKing = Board.GetBlackKingPosition();
        if (IsGainingOppositionFast(to, blackKing, from)) return true;

        // Moving closer to enemy pawns using fast distance lookup
        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);
        if (blackPawns.Any() && GetDistanceToNearestPawnFast(to, blackPawns) < GetDistanceToNearestPawnFast(from, blackPawns))
            return true;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackKingImprovedActivity(byte from, byte to)
    {
        var fromCentralization = _centralizationValues[from];
        var toCentralization = _centralizationValues[to];

        if (toCentralization > fromCentralization) return true;

        var whiteKing = Board.GetWhiteKingPosition();
        if (IsGainingOppositionFast(to, whiteKing, from)) return true;

        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);
        if (whitePawns.Any() && GetDistanceToNearestPawnFast(to, whitePawns) < GetDistanceToNearestPawnFast(from, whitePawns))
            return true;

        return false;
    }

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

    // OPTIMIZED: Bishop endgame improvements using precomputed key squares
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWhiteBishopImprovedEndgame(byte from, byte to)
    {
        // Improved mobility
        var fromMobility = to.BishopAttacks(Board.GetOccupied()).Count();
        var toMobility = from.BishopAttacks(Board.GetOccupied()).Count();
        if (fromMobility < toMobility) return true;

        // Use precomputed bishop key squares for ultra-fast control checking
        var blackPawns = Board.GetPieceBits(Pieces.BlackPawn);
        if (blackPawns.Any() && IsBishopControllingKeySquaresFast(to, blackPawns)) return true;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBlackBishopImprovedEndgame(byte from, byte to)
    {
        var fromMobility = to.BishopAttacks(Board.GetOccupied()).Count();
        var toMobility = from.BishopAttacks(Board.GetOccupied()).Count();
        if (fromMobility < toMobility) return true;

        var whitePawns = Board.GetPieceBits(Pieces.WhitePawn);
        if (whitePawns.Any() && IsBishopControllingKeySquaresFast(to, whitePawns)) return true;

        return false;
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
        // Use precomputed centralization values for instant comparison
        var fromCentralization = _centralizationValues[from];
        var toCentralization = _centralizationValues[to];

        if (toCentralization > fromCentralization) return true;

        // Check for strong outpost improvement
        var fromOutpost = (_knightOutposts[from] & from.AsBitBoard()).Any();
        var toOutpost = (_knightOutposts[to] & to.AsBitBoard()).Any();

        return toOutpost && !fromOutpost;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsKnightBlockingEnemyPawns(byte square, bool isBlack)
    {
        var enemyPawns = isBlack ? Board.GetPieceBits(Pieces.WhitePawn) : Board.GetPieceBits(Pieces.BlackPawn);
        return enemyPawns.Any() && GetDistanceToNearestPawnFast(square, enemyPawns) <= 2;
    }

    // OPTIMIZED: Rook endgame activity using precomputed king zones
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsRookCuttingOffEnemyKing(byte square, bool isBlack)
    {
        var enemyKing = isBlack ? Board.GetWhiteKingPosition() : Board.GetBlackKingPosition();
        var rookAttacks = square.RookAttacks(Board.GetOccupied());

        // Use precomputed distance table for instant distance check
        return (rookAttacks & enemyKing.AsBitBoard()).IsZero() &&
               _manhattanDistances[square][enemyKing] <= 4;
    }

    // Queen activity
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsQueenImprovedActivity(byte from, byte to)
    {
        var fromMobility = from.QueenAttacks(Board.GetOccupied()).Count();
        var toMobility = to.QueenAttacks(Board.GetOccupied()).Count();

        return toMobility > fromMobility;
    }

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
    private bool IsGainingOppositionFast(byte kingTo, byte enemyKing, byte kingFrom)
    {
        // Use precomputed distance tables for instant opposition detection
        var oldDistance = _chebyshevDistances[kingFrom][enemyKing];
        var newDistance = _chebyshevDistances[kingTo][enemyKing];

        // Gaining opposition if distance becomes exactly 2 (direct opposition)
        return newDistance == 2 && oldDistance != 2;
    }

    // Existing methods remain unchanged
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetOpeningMoves() => AttackCollection.BuildOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetBookOpeningMoves() => AttackCollection.BuildBookOpening();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetBookMiddleMoves() => AttackCollection.BuildBookMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetMiddleMoves() => AttackCollection.BuildMiddle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetEndMoves() => AttackCollection.BuildEnd();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override MoveList GetBookEndMoves() => AttackCollection.BuildBookEnd();
}