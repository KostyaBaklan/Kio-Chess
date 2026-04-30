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
    public short UnstoppablePassedPawnValue { get; set; }
    public byte OutsidePassedPawnValue { get; set; }
    public byte CenterAttackValue { get; set; }
    public byte ExtendedCenterAttackValue { get; set; }
    public byte TrappedPieceThreshold { get; set; }
    public byte[] TrappedKnightPenalties { get; set; }
    public byte[] TrappedBishopPenalties { get; set; }
    public byte[] TrappedRookPenalties { get; set; }
    public byte BadBishopPenalty { get; set; }
    public byte BadBishopThreshold { get; set; }
    public byte FixedCenterPawnPenalty { get; set; }
    public byte MinorDefenseBonus { get; set; }
    public byte CentralPieceDefenseBonus { get; set; }

    // Development tracking
    public byte DevelopmentPenalty { get; set; }
    public byte DevelopmentThresholdMove { get; set; }

    // Outpost evaluation - file-indexed using FileBuffer (8 values: A-H)
    public byte[] KnightOutpostRank5 { get; set; }  // 8 values (one per file A-H)
    public byte[] KnightOutpostRank6 { get; set; }  // 8 values (one per file A-H)
    public byte[] BishopOutpostRank5 { get; set; }  // 8 values (one per file A-H)
    public byte[] BishopOutpostRank6 { get; set; }  // 8 values (one per file A-H)
    public byte OutpostDefendedByPawnBonus { get; set; }

    // Rook on 7th rank evaluation
    public byte RookOn7thRankBonus { get; set; }
    public byte RookOn7thWithKingOn8thBonus { get; set; }
    public byte DoubledRooksOn7thBonus { get; set; }
    public byte QueenRookOn7thBonus { get; set; }  // Queen + Rook synergy

    // Endgame king evaluation
    public byte DirectOppositionBonus { get; set; }
    public byte DiagonalOppositionBonus { get; set; }
    public byte DistantOppositionBonus { get; set; }

    // Outside passed pawn evaluation
    public byte OutsidePassedPawnBonus { get; set; }
    public byte OutsidePassedPawnAdvancedBonus { get; set; }  // Per rank
    public byte OutsidePassedPawnKingDistanceBonus { get; set; }  // Per file distance

    // Pawn chain evaluation
    public byte[] PawnChainBonusByLength { get; set; }  // Bonus indexed by chain length [0..7+]

    // Pawn majority evaluation
    public byte PawnMajorityBonus { get; set; }  // Bonus per extra pawn in wing majority
    public byte PawnMajorityKingDistanceFactor { get; set; }  // Penalty per square if king far from majority (endgame only)

    // Rook activity in endgame
    public byte RookIndependenceFactor { get; set; }  // +2cp per file distance from own king
    public byte RookCuttingOffKingBonus { get; set; }  // +30cp if cutting off enemy king
    public byte ActiveRookBonus { get; set; }  // +15cp if rook on 7th/8th rank
    public byte PassiveRookPenalty { get; set; }  // -10cp if defending on 1st/2nd rank

    // Key square control (endgame)
    public byte KeySquareControlBonus { get; set; }  // +30cp for occupying key square
    public byte KeySquareProximityFactor { get; set; }  // +5cp per square closer to key squares

    // Fianchetto structure evaluation
    public sbyte FianchettoBonus { get; set; }  // +15-20cp for complete fianchetto structure
    public sbyte FianchettoWithoutBishopPenalty { get; set; }  // -10-15cp if pawn advanced but bishop missing
    public byte FianchettoKingSafetyBonus { get; set; }  // +10-12cp if king castled to fianchetto side
    public sbyte FianchettoBishopTradedPenalty { get; set; }  // -20-25cp if bishop traded after fianchetto

    // Castle rights evaluation
    public byte CastleRightsBothBonus { get; set; }  // Bonus for preserving both castling sides
    public byte CastleRightsOneBonus { get; set; }   // Bonus for preserving one castling side

    // Open file near king evaluation
    public byte OpenFileNearKingPenalty { get; set; }      // Penalty for open file adjacent to king
    public byte HalfOpenFileNearKingPenalty { get; set; }  // Penalty for half-open file adjacent to king

    // Tempo bonus
    public byte TempoBonus { get; set; }  // Bonus for having the move (initiative)

    // Early queen development penalty
    public byte EarlyQueenPenalty { get; set; }          // Penalty for moving queen early
    public byte EarlyQueenMinorPieceThreshold { get; set; }  // Min minor pieces that should be developed first

    // Hanging piece detection (indexed by piece type: [WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen, WhiteKing])
    public short[] HangingPiecePenalties { get; set; }   // Pre-calculated penalties for hanging pieces (50% of piece value)
}