namespace Engine.Models.Config;

public class BoardEvaluation
{
    public short DoubleBishopValue { get; set; }
    public short BlockedPawnValue { get; set; }
    public short DoubledPawnValue { get; set; }
    public short IsolatedPawnValue { get; set; }
    public short BackwardPawnValue { get; set; }
    public short RookOnOpenFileValue { get; set; }
    public short RentgenValue { get; set; }
    public int RookOnHalfOpenFileValue { get; set; }
    public int RookBlockedByKingValue { get; set; }
    public int NoPawnsValue { get; set; }
    public byte[] MobilityValues { get; set; }
    public byte RookOnOpenFileNextToKingValue { get; set; }
    public byte DoubleRookOnOpenFileValue { get; set; }
    public byte RookOnHalfOpenFileNextToKingValue { get; set; }
    public byte DoubleRookOnHalfOpenFileValue { get; set; }
    public byte ConnectedRooksOnFirstRankValue { get; set; }
    public byte DiscoveredCheckValue { get; set; }
    public byte DiscoveredAttackValue { get; set; }
    public byte AbsolutePinValue { get; set; }
    public byte PartialPinValue { get; set; }
    public byte BishopBattaryValue { get; set; }
    public byte RookBattaryValue { get; set; }
    public byte QueenBattaryValue { get; set; }
    public byte RookBehindPassedPawnValue { get; set; }

    /// <summary>
    /// Bonus for an unstoppable passed pawn (enemy king outside the "square of the pawn").
    /// This is a significant bonus as such a pawn will promote.
    /// </summary>
    public short UnstoppablePassedPawnValue { get; set; }

    /// <summary>
    /// Bonus for a passed pawn on an outside file (A, B, G, H) in endgame.
    /// Outside passed pawns divert the enemy king, allowing friendly king to penetrate.
    /// </summary>
    public byte OutsidePassedPawnValue { get; set; }

    /// <summary>
    /// Bonus for winning the pawn race in endgame positions.
    /// When one side's passed pawn will promote before the opponent's,
    /// this bonus is awarded to the side that promotes first.
    /// The value should be significant (typically 100-200 centipawns)
    /// as tempo advantage in pawn races is often decisive.
    /// </summary>
    public short PawnRaceWinnerBonus { get; set; }

    /// <summary>
    /// Bonus for having the opposition in K+P endgames.
    /// Opposition means kings are on the same file/rank/diagonal with exactly
    /// one square between them, and it's the opponent's turn to move.
    /// The side with opposition can force the enemy king to give way.
    /// </summary>
    public byte OppositionBonus { get; set; }

    /// <summary>
    /// Bonus for a rook on the 7th rank (2nd rank for black) in endgame.
    /// A rook on the 7th rank attacks pawns on their starting squares
    /// and restricts the enemy king to the back rank.
    /// </summary>
    public byte RookOn7thRankValue { get; set; }

    /// <summary>
    /// Bonus for a rook cutting off the enemy king from the action.
    /// When a rook controls a file/rank that prevents the enemy king
    /// from reaching key squares (e.g., supporting passed pawns).
    /// </summary>
    public byte RookCuttingOffKingValue { get; set; }

    /// <summary>
    /// Bonus for king centralization in queenless endgames.
    /// A centralized king is more active and can support pawns or attack enemy pawns.
    /// The bonus is scaled by how central the king is (d4/d5/e4/e5 = max, corners = 0).
    /// </summary>
    public byte KingCentralizationValue { get; set; }

    /// <summary>
    /// Bonus for the friendly king occupying a key square relative to a passed pawn.
    /// Key squares are squares that, if occupied by the friendly king, guarantee pawn promotion.
    /// For pawns on ranks 2-4: key squares are 2 ranks ahead (3 squares wide).
    /// For pawns on ranks 5-6: key squares extend to the promotion rank.
    /// This bonus is only applied in K+P and late endgames.
    /// </summary>
    public byte KingOnKeySquareValue { get; set; }

    /// <summary>
    /// Bonus for a knight on an outpost square in endgame.
    /// An outpost is a square protected by a friendly pawn that cannot be attacked by enemy pawns.
    /// Knights on outposts are very stable and control key squares.
    /// </summary>
    public byte KnightOutpostValue { get; set; }

    /// <summary>
    /// Bonus for a knight blocking an enemy passed pawn in endgame.
    /// Knights are excellent blockers because they cannot be driven away by the pawn.
    /// </summary>
    public byte KnightBlockadeValue { get; set; }

    /// <summary>
    /// Bonus for a bishop on one of the long diagonals (a1-h8 or a8-h1) in endgame.
    /// Bishops on long diagonals control the maximum number of squares and
    /// can influence both sides of the board.
    /// </summary>
    public byte BishopLongDiagonalValue { get; set; }

    /// <summary>
    /// Scaling factor (0-100) applied to evaluation when opposite-colored bishops are present.
    /// Opposite-colored bishop endgames are highly drawish because the bishops
    /// cannot contest each other's squares. A value of 50 means the evaluation
    /// is reduced by 50% (multiplied by 0.5). Only applied in endgames with pawns.
    /// </summary>
    public byte OppositeColoredBishopDrawFactor { get; set; }

    /// <summary>
    /// Penalty for having a "wrong-colored" bishop with only rook pawn(s) on one side.
    /// A bishop that cannot control the promotion square of a rook pawn (A or H file)
    /// often results in a drawn position even with an extra pawn.
    /// For example: White has a light-squared bishop and an A-pawn (promotes on dark a8).
    /// </summary>
    public byte WrongColoredBishopPenalty { get; set; }

    /// <summary>
    /// Bonus for a minor piece (knight or bishop) controlling the promotion square
    /// of an enemy passed pawn. This is valuable because it can prevent the pawn
    /// from promoting even if the piece is sacrificed.
    /// </summary>
    public byte MinorControlsPromotionSquareValue { get; set; }

    /// <summary>
    /// Bonus for a bishop on an outpost square (protected by pawn, cannot be attacked by enemy pawns).
    /// Less common than knight outposts, but valuable when the bishop controls key diagonals
    /// from a stable position.
    /// </summary>
    public byte BishopOutpostValue { get; set; }
}