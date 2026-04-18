using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.DataAccess.Interfaces;
using Engine.Models.Moves;
using Engine.Models.Helpers;
using Engine.Models.Enums;
using Engine.Services;
using System.Text;

namespace Analysis.Core.Services;

/// <summary>
/// Service for analyzing chess games and positions, providing detailed move-by-move evaluation.
/// </summary>
public class AnalysisService : IAnalysisService
{
    private readonly IStockfishService _stockfish;

    public AnalysisService(IStockfishService stockfish)
    {
        _stockfish = stockfish;
    }

    /// <summary>
    /// Analyzes a single position at the specified depth.
    /// </summary>
    public async Task<PositionAnalysis> AnalysePositionAsync(string fen, int depth, CancellationToken ct = default)
    {
        var info = await _stockfish.AnalysePositionAsync(fen, depth, ct);
        
        return new PositionAnalysis
        {
            Fen = fen,
            Depth = info.Depth,
            Evaluation = info.Centipawns,
            IsMate = info.IsMate,
            MateIn = info.MateIn,
            BestMove = info.BestMove,
            PrincipalVariation = info.Pv,
            Nodes = info.Nodes,
            TimeMs = info.Time
        };
    }

    /// <summary>
    /// Analyzes an entire game move-by-move and returns a comprehensive report.
    /// </summary>
    public async Task<GameAnalysisReport> AnalyseGameAsync(
        List<MoveBase> moves, 
        int depth = 18, 
        IProgress<AnalysisProgress> progress = null,
        CancellationToken ct = default)
    {
        return await AnalyseGameAsync(moves, null, depth, progress, ct);
    }

    /// <summary>
    /// Analyzes an entire game move-by-move with opening book detection.
    /// </summary>
    public async Task<GameAnalysisReport> AnalyseGameAsync(
        List<MoveBase> moves,
        IOpeningExplorerService openingExplorer,
        int depth = 18,
        IProgress<AnalysisProgress> progress = null,
        CancellationToken ct = default)
    {
        var report = new GameAnalysisReport
        {
            TotalMoves = moves.Count,
            AnalysisDepth = depth,
            AnalysisDate = DateTime.Now
        };

        var analysedMoves = new List<AnalysedMove>();
        
        // Create temporary Position for phase detection and FEN building
        // Backup MoveBase.Board to avoid corrupting main app's Board
        var previousBoard = Engine.Models.Moves.MoveBase.Board;
        Engine.Models.Boards.Position tempPosition = null;
        
        try
        {
            tempPosition = new Engine.Models.Boards.Position();
            tempPosition.Clear();

            for (int i = 0; i < moves.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                progress?.Report(new AnalysisProgress
                {
                    CurrentMove = i + 1,
                    TotalMoves = moves.Count,
                    Status = $"Analyzing move {i + 1}/{moves.Count}..."
                });

                var move = moves[i];
                var isWhite = i % 2 == 0;

                // Build move sequence before this move
                var movesBeforeThis = moves.Take(i).ToList();
                string uciSeqBefore = UciMoveConverter.BuildMoveSequence(movesBeforeThis);
                string playedUci = UciMoveConverter.ToUci(move);

                // Build position key for opening book lookup (all moves including this one)
                var allMovesUpToThis = moves.Take(i + 1).ToList();
                string positionKey = string.Join("_", allMovesUpToThis.Select(m => UciMoveConverter.ToUci(m)));

                // Check if this move is in the opening book FIRST
                bool isBookMove = false;
                if (openingExplorer != null)
                {
                    try
                    {
                        var opening = await openingExplorer.GetOpeningByPositionAsync(positionKey);
                        isBookMove = opening != null;
                    }
                    catch
                    {
                        // If opening lookup fails, continue with engine evaluation
                        isBookMove = false;
                    }
                }

                MoveClassification classification;
                MoveEvaluation evaluation;

                if (isBookMove)
                {
                    // Book move - don't evaluate with engine, just mark as Book
                    // Still need to evaluate position for display purposes
                    var basicEval = await _stockfish.AnalysePositionAsync(
                        BuildFenAfterMoves(tempPosition, allMovesUpToThis), 
                        10, 
                        ct);
                    
                    evaluation = new MoveEvaluation
                    {
                        EvalBefore = 0,
                        EvalAfter = basicEval.Centipawns,
                        BestMoveEval = basicEval.Centipawns,
                        BestMove = string.Empty,
                        PlayedMove = playedUci,
                        BestMoveSequence = string.Empty,
                        Classification = MoveClassification.Book,
                    };
                    classification = MoveClassification.Book;
                }
                else
                {
                    // Regular engine evaluation
                    evaluation = await _stockfish.EvaluateMoveAsync(uciSeqBefore, playedUci, depth, ct);
                    classification = evaluation.Classification;
                }

                // Make the move on temp position to track phase
                if (i == 0)
                    tempPosition.MakeFirst(move);
                else
                    tempPosition.Make(move);

                // Get actual game phase from MoveHistoryService (material-based)
                var phase = GetGamePhase(tempPosition);

                var analysedMove = new AnalysedMove
                {
                    MoveNumber = (i / 2) + 1,
                    IsWhite = isWhite,
                    Move = move,
                    Notation = FormatMoveNotation(move),
                    EvaluationBefore = evaluation.EvalBefore,
                    EvaluationAfter = evaluation.EvalAfter,
                    BestMove = evaluation.BestMove,
                    BestMoveSequence = evaluation.BestMoveSequence,
                    BestMoveEvaluation = evaluation.BestMoveEval,
                    Classification = classification,
                    CentipawnLoss = evaluation.CentipawnLoss,
                    Phase = phase
                };

                analysedMoves.Add(analysedMove);

                // Update statistics
                UpdateStatistics(report, analysedMove, isWhite);
            }

            report.Moves = analysedMoves;
            CalculatePhaseScores(report);
            DetermineKeyMoments(report);
        }
        finally
        {
            // Restore original Board reference
            Engine.Models.Moves.MoveBase.Board = previousBoard;
        }

        return report;
    }

    private string BuildFenAfterMoves(Engine.Models.Boards.Position position, List<MoveBase> moves)
    {
        // The position has already had all moves applied to it
        // We need to build FEN from current position state
        
        var board = position.GetBoard();
        var sb = new StringBuilder();
        
        // Build piece placement (ranks 8 to 1)
        for (int rank = 7; rank >= 0; rank--)
        {
            int emptyCount = 0;
            for (int file = 0; file < 8; file++)
            {
                byte square = (byte)(rank * 8 + file);
                position.GetPiece(square, out byte? piece);
                
                if (piece == null)
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        sb.Append(emptyCount);
                        emptyCount = 0;
                    }
                    sb.Append(GetFenPieceChar(piece.Value));
                }
            }
            
            if (emptyCount > 0)
                sb.Append(emptyCount);
            
            if (rank > 0)
                sb.Append('/');
        }
        
        // Active color
        var turn = position.GetTurn();
        sb.Append(turn == Engine.Models.Enums.Turn.White ? " w " : " b ");
        
        // Castling rights (simplified - assume all available if not moved)
        sb.Append("KQkq ");
        
        // En passant (simplified - none)
        sb.Append("- ");
        
        // Halfmove clock (simplified)
        sb.Append("0 ");
        
        // Fullmove number
        int fullMoveNumber = (moves.Count / 2) + 1;
        sb.Append(fullMoveNumber);
        
        return sb.ToString();
    }

    private char GetFenPieceChar(byte piece)
    {
        return piece switch
        {
            0 => 'P',  // WhitePawn
            1 => 'N',  // WhiteKnight
            2 => 'B',  // WhiteBishop
            3 => 'R',  // WhiteRook
            4 => 'Q',  // WhiteQueen
            5 => 'K',  // WhiteKing
            6 => 'p',  // BlackPawn
            7 => 'n',  // BlackKnight
            8 => 'b',  // BlackBishop
            9 => 'r',  // BlackRook
            10 => 'q', // BlackQueen
            11 => 'k', // BlackKing
            _ => '?'
        };
    }

    private GamePhase GetGamePhase(Engine.Models.Boards.Position position)
    {
        // Get phase from MoveHistoryService which uses material-based detection
        var historyService = ContainerLocator.Current.Resolve<MoveHistoryService>();
        
        byte phase = historyService.GetPhase();
        
        return phase switch
        {
            Phase.Opening => GamePhase.Opening,
            Phase.Middle => GamePhase.Middlegame,
            Phase.End => GamePhase.Endgame,
            _ => GamePhase.Opening
        };
    }

    private GamePhase DeterminePhase(int moveIndex, int totalMoves)
    {
        // Fallback phase detection if MoveHistoryService not available
        // Opening: First 20 plies (10 moves per side)
        // Endgame: Last 20 plies (10 moves per side)  
        // Middlegame: Everything in between
        
        if (moveIndex < 20) 
            return GamePhase.Opening;
        
        if (totalMoves >= 40 && moveIndex >= totalMoves - 20)
            return GamePhase.Endgame;
        
        return GamePhase.Middlegame;
    }

    private string FormatMoveNotation(MoveBase move)
    {
        // Basic notation - in production, use IMoveFormatter
        if (move.IsCastle)
            return move.To > move.From ? "O-O" : "O-O-O";

        var from = move.From.AsString();
        var to = move.To.AsString();
        
        return $"{from}{to}";
    }

    private void UpdateStatistics(GameAnalysisReport report, AnalysedMove move, bool isWhite)
    {
        var stats = isWhite ? report.WhiteStatistics : report.BlackStatistics;

        stats.TotalMoves++;

        switch (move.Classification)
        {
            case MoveClassification.Best:
                stats.BestMoves++;
                break;
            case MoveClassification.Excellent:
                stats.ExcellentMoves++;
                break;
            case MoveClassification.Good:
                stats.GoodMoves++;
                break;
            case MoveClassification.Brilliant:
                stats.BrilliantMoves++;
                break;
            case MoveClassification.Book:
                stats.BookMoves++;
                break;
            case MoveClassification.Inaccuracy:
                stats.Inaccuracies++;
                break;
            case MoveClassification.Mistake:
                stats.Mistakes++;
                break;
            case MoveClassification.Blunder:
                stats.Blunders++;
                break;
        }

        stats.TotalCentipawnLoss += move.CentipawnLoss;
        stats.AverageCentipawnLoss = stats.TotalMoves > 0 
            ? stats.TotalCentipawnLoss / stats.TotalMoves 
            : 0;

        // Calculate accuracy (chess.com formula approximation)
        stats.Accuracy = CalculateAccuracy(stats);
    }

    private double CalculateAccuracy(PlayerStatistics stats)
    {
        if (stats.TotalMoves == 0) return 0;

        // Simplified accuracy calculation
        // Best moves = 100%, Excellent = 95%, Good = 90%, Book = 100%, etc.
        double accuracySum = 
            stats.BestMoves * 100 +
            stats.ExcellentMoves * 95 +
            stats.GoodMoves * 90 +
            stats.BrilliantMoves * 100 +
            stats.BookMoves * 100 +
            stats.Inaccuracies * 70 +
            stats.Mistakes * 40 +
            stats.Blunders * 10;

        return Math.Round(accuracySum / stats.TotalMoves, 1);
    }

    private void CalculatePhaseScores(GameAnalysisReport report)
    {
        var openingMoves = report.Moves.Where(m => m.Phase == GamePhase.Opening).ToList();
        var middlegameMoves = report.Moves.Where(m => m.Phase == GamePhase.Middlegame).ToList();
        var endgameMoves = report.Moves.Where(m => m.Phase == GamePhase.Endgame).ToList();

        report.PhaseScores.OpeningScore = CalculatePhaseScore(openingMoves);
        report.PhaseScores.MiddlegameScore = CalculatePhaseScore(middlegameMoves);
        report.PhaseScores.EndgameScore = CalculatePhaseScore(endgameMoves);
        
        // Calculate per-player phase accuracies
        report.PhaseScores.WhiteOpeningAccuracy = CalculatePhaseAccuracy(openingMoves.Where(m => m.IsWhite).ToList());
        report.PhaseScores.BlackOpeningAccuracy = CalculatePhaseAccuracy(openingMoves.Where(m => !m.IsWhite).ToList());
        report.PhaseScores.WhiteMiddlegameAccuracy = CalculatePhaseAccuracy(middlegameMoves.Where(m => m.IsWhite).ToList());
        report.PhaseScores.BlackMiddlegameAccuracy = CalculatePhaseAccuracy(middlegameMoves.Where(m => !m.IsWhite).ToList());
        report.PhaseScores.WhiteEndgameAccuracy = CalculatePhaseAccuracy(endgameMoves.Where(m => m.IsWhite).ToList());
        report.PhaseScores.BlackEndgameAccuracy = CalculatePhaseAccuracy(endgameMoves.Where(m => !m.IsWhite).ToList());
    }

    private double CalculatePhaseScore(List<AnalysedMove> moves)
    {
        if (moves.Count == 0) return 0;

        int goodMoves = moves.Count(m => m.Classification == MoveClassification.Best || 
                                          m.Classification == MoveClassification.Excellent ||
                                          m.Classification == MoveClassification.Good ||
                                          m.Classification == MoveClassification.Brilliant ||
                                          m.Classification == MoveClassification.Book);

        return Math.Round((double)goodMoves / moves.Count * 100, 1);
    }

    private double CalculatePhaseAccuracy(List<AnalysedMove> moves)
    {
        if (moves.Count == 0) return 0;

        // Use same calculation as overall accuracy
        double accuracySum = 
            moves.Count(m => m.Classification == MoveClassification.Best) * 100 +
            moves.Count(m => m.Classification == MoveClassification.Excellent) * 95 +
            moves.Count(m => m.Classification == MoveClassification.Good) * 90 +
            moves.Count(m => m.Classification == MoveClassification.Brilliant) * 100 +
            moves.Count(m => m.Classification == MoveClassification.Book) * 100 +
            moves.Count(m => m.Classification == MoveClassification.Inaccuracy) * 70 +
            moves.Count(m => m.Classification == MoveClassification.Mistake) * 40 +
            moves.Count(m => m.Classification == MoveClassification.Blunder) * 10;

        return Math.Round(accuracySum / moves.Count, 1);
    }

    private void DetermineKeyMoments(GameAnalysisReport report)
    {
        report.KeyMoments = new List<KeyMoment>();

        // Find brilliant moves
        var brilliantMoves = report.Moves
            .Where(m => m.Classification == MoveClassification.Brilliant)
            .Select(m => new KeyMoment
            {
                MoveNumber = m.MoveNumber,
                IsWhite = m.IsWhite,
                Type = KeyMomentType.Brilliant,
                Description = $"Brilliant move! {m.Notation}",
                Move = m
            });

        // Find blunders
        var blunders = report.Moves
            .Where(m => m.Classification == MoveClassification.Blunder)
            .Select(m => new KeyMoment
            {
                MoveNumber = m.MoveNumber,
                IsWhite = m.IsWhite,
                Type = KeyMomentType.Blunder,
                Description = $"Blunder: {m.Notation} (lost {m.CentipawnLoss} centipawns)",
                Move = m
            });

        // Find turning points (large evaluation swings)
        var turningPoints = new List<KeyMoment>();
        for (int i = 1; i < report.Moves.Count; i++)
        {
            var prevEval = report.Moves[i - 1].EvaluationAfter;
            var currEval = report.Moves[i].EvaluationAfter;
            var swing = Math.Abs(currEval - prevEval);

            if (swing > 200) // Large swing
            {
                turningPoints.Add(new KeyMoment
                {
                    MoveNumber = report.Moves[i].MoveNumber,
                    IsWhite = report.Moves[i].IsWhite,
                    Type = KeyMomentType.TurningPoint,
                    Description = $"Turning point at move {report.Moves[i].MoveNumber}",
                    Move = report.Moves[i]
                });
            }
        }

        report.KeyMoments.AddRange(brilliantMoves);
        report.KeyMoments.AddRange(blunders);
        report.KeyMoments.AddRange(turningPoints);
        report.KeyMoments = report.KeyMoments.OrderBy(k => k.MoveNumber).ToList();
    }
}

public class AnalysisProgress
{
    public int CurrentMove { get; set; }
    public int TotalMoves { get; set; }
    public string Status { get; set; } = string.Empty;
    public double PercentComplete => TotalMoves > 0 ? (double)CurrentMove / TotalMoves * 100 : 0;
}
