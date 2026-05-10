using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {
        private short[] _whiteKingShieldLookup;
        private short[] _blackKingShieldLookup;
        private CellBuffer<BitBoard> _whiteKingShieldMask;
        private CellBuffer<BitBoard> _blackKingShieldMask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKingOpening()
        {
            return _evaluationService.GetWhiteKingFullValue(_whiteKingPosition)
                + WhiteKingShieldOpeningValue(_whiteKingPosition)
                + WhiteKingZoneAttack();
            //- WhiteKingOpenValue(kingPosition);
            //- WhiteKingAttackValue(kingPosition);
            //+ WhiteDistanceToQueen(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKingMiddle()
        {
            return _evaluationService.GetWhiteKingFullValue(_whiteKingPosition)
                + WhiteKingShieldMiddleValue(_whiteKingPosition)
                + WhiteKingZoneAttack();
            //- WhiteKingOpenValue(kingPosition);
            //- WhiteKingAttackValue(kingPosition)
            //+ WhiteDistanceToQueen(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKingEnd()
        {
            return _evaluationService.GetWhiteKingFullValue(_whiteKingPosition)
                - KingPawnTrofism(_whiteKingPosition);
            //+ WhiteDistanceToQueen(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKingOpening()
        {
            return _evaluationService.GetBlackKingFullValue(_blackKingPosition)
                + BlackKingShieldOpeningValue(_blackKingPosition)
                + BlackKingZoneAttack();
            //- BlackKingOpenValue(kingPosition);
            //- BlackKingAttackValue(kingPosition)
            // BlackDistanceToQueen(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKingMiddle()
        {
            return _evaluationService.GetBlackKingFullValue(_blackKingPosition)
                + BlackKingShieldMiddleValue(_blackKingPosition)
                + BlackKingZoneAttack();
            //- BlackKingOpenValue(kingPosition);
            //- BlackKingAttackValue(kingPosition);
            //BlackDistanceToQueen(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKingEnd()
        {
            return _evaluationService.GetBlackKingFullValue(_blackKingPosition)
                - KingPawnTrofism(_blackKingPosition);
            //+ BlackDistanceToQueen(kingPosition);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingZoneAttack()
        {
            int valueOfAttacks = 0;
            BitBoard attackPattern;
            BitBoardList boards = stackalloc BitBoard[10];

            var bits = _boards[Pieces.WhiteKnight];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _whiteKnightPatterns[position] & _blackKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            bits = _boards[Pieces.WhiteBishop];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _whiteBishopAttacks[position] & _blackKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            bits = _boards[Pieces.WhiteRook];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _whiteRookAttacks[position] & _blackKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            bits = _boards[Pieces.WhiteQueen];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _whiteQueenAttacks[position] & _blackKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetQueenAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            if (boards.Count < 1) return 0;

            attackPattern = _whitePawnAttacks & _blackKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks++;
                boards.Add(attackPattern);
                return boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
            }

            return boards.Count < 2
                ? 0
                : boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingZoneAttack()
        {
            int valueOfAttacks = 0;
            BitBoard attackPattern;
            BitBoardList boards = stackalloc BitBoard[10];

            var bits = _boards[Pieces.BlackKnight];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _blackKnightPatterns[position] & _whiteKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            bits = _boards[Pieces.BlackBishop];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _blackBishopAttacks[position] & _whiteKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetBishopAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            bits = _boards[Pieces.BlackRook];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _blackRookAttacks[position] & _whiteKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetRookAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            bits = _boards[Pieces.BlackQueen];
            while (bits.Any())
            {
                var position = bits.BitScanForward();
                attackPattern = _blackQueenAttacks[position] & _whiteKingZone;
                if (attackPattern.Any())
                {
                    valueOfAttacks += attackPattern.Count() * _evaluationService.GetQueenAttackValue();
                    boards.Add(attackPattern);
                }
                bits = bits.Remove(position);
            }

            if (boards.Count < 1) return 0;

            attackPattern = _blackPawnAttacks & _whiteKingZone;
            if (attackPattern.Any())
            {
                valueOfAttacks++;
                boards.Add(attackPattern);
                return boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
            }

            return boards.Count < 2
                ? 0
                : boards.GetKingZoneWeight(valueOfAttacks * _evaluationService.GetAttackWeight(boards.Count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetDistance(byte from, BitBoard bits)
        {
            int value = 0;
            var distance = _evaluationService.Distance(from);
            while (bits.Any())
            {
                byte position = bits.BitScanForward();
                value += distance[position];
                bits = bits.Remove(position);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int KingPawnTrofism(byte kingPosition) => _trofismCoefficient * GetDistance(kingPosition, _boards[0] | _boards[6]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingShieldOpeningValue(byte kingPosition) => _moveHistory.CanDoBlackCastle() ? 0 : BlackKingShieldMiddleValue(kingPosition);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingShieldMiddleValue(byte kingPosition)
        {
            short pattern = _boards[Pieces.BlackPawn].ExtractBits(_blackKingShieldMask[kingPosition]);
            return _blackKingShieldLookup[kingPosition * 512 + pattern];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingShieldOpeningValue(byte kingPosition) => _moveHistory.CanDoWhiteCastle() ? 0 : WhiteKingShieldMiddleValue(kingPosition);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingShieldMiddleValue(byte kingPosition)
        {
            short pattern = _boards[Pieces.WhitePawn].ExtractBits(_whiteKingShieldMask[kingPosition]);
            return _whiteKingShieldLookup[kingPosition * 512 + pattern];
        }

        #region Pawn Shield

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingShield(byte kingPosition, BitBoard pawns)
        {
            return (_whitePawnShield2[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
                (_whitePawnShield3[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
                (_whitePawnShield4[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
                (_whitePawnKingShield2[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
                (_whitePawnKingShield3[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
                (_whitePawnKingShield4[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingShield(byte kingPosition, BitBoard pawns)
        {
            return (_blackPawnShield7[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield2Value() +
                            (_blackPawnShield6[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield3Value() +
                            (_blackPawnShield5[kingPosition] & pawns).Count() * _evaluationService.GetPawnShield4Value() +
                            (_blackPawnKingShield7[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield2Value() +
                            (_blackPawnKingShield6[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield3Value() +
                            (_blackPawnKingShield5[kingPosition] & pawns).Count() * _evaluationService.GetKingPawnShield4Value();
        }

        private void InitializeKingEvaluation()
        {
            _evaluationService = _evaluationServiceFactory.GetEvaluationService(0);
            _whiteKingShieldLookup = new short[64 * 512];
            _blackKingShieldLookup = new short[64 * 512];
            _whiteKingShieldMask = new CellBuffer<BitBoard>();
            _blackKingShieldMask = new CellBuffer<BitBoard>();

            for (byte kingPos = 0; kingPos < 64; kingPos++)
            {
                GenerateWhiteShieldLookup(kingPos);
                GenerateBlackShieldLookup((byte)(63 - kingPos));
            }
        }

        private void GenerateBlackShieldLookup(byte kingPos)
        {
            int file = kingPos % 8;
            int rank = kingPos / 8;

            // Build list of shield squares (up to 9)
            List<byte> shieldSquares = new List<byte>();

            // Only generate shield if king is on upper ranks
            if (rank > 1) // King on ranks 2-7
            {
                // Add squares from adjacent files and king file, ranks -1, -2, -3
                for (int r = rank - 1; r >= Math.Max(0, rank - 3); r--)
                {
                    // Left file (if exists)
                    if (file > 0)
                        shieldSquares.Add((byte)(r * 8 + file - 1));

                    // King file
                    shieldSquares.Add((byte)(r * 8 + file));

                    // Right file (if exists)
                    if (file < 7)
                        shieldSquares.Add((byte)(r * 8 + file + 1));
                }
            }

            // Create mask from shield squares
            BitBoard mask = new BitBoard(0);
            foreach (var sq in shieldSquares)
            {
                mask = mask.Set(sq);
            }
            _blackKingShieldMask[kingPos] = mask;

            var maxP = mask.ExtractBits(mask) + 1;

            // Generate all 512 possible pawn patterns
            for (int pattern = 0; pattern < maxP; pattern++)
            {
                // Reconstruct pawn bitboard from pattern
                BitBoard pawns = new BitBoard(0);
                for (int i = 0; i < shieldSquares.Count && i < 9; i++)
                {
                    if ((pattern & (1 << i)) != 0)
                    {
                        pawns = pawns.Set(shieldSquares[i]);
                    }
                }

                // Calculate score
                short score = (short)BlackKingShield(kingPos, pawns);

                var p = pawns.ExtractBits(mask);


                _blackKingShieldLookup[kingPos * 512 + p] = score;
            }
        }

        private void GenerateWhiteShieldLookup(byte kingPos)
        {
            int file = kingPos % 8;
            int rank = kingPos / 8;

            // Build list of shield squares (up to 9)
            List<byte> shieldSquares = new List<byte>();

            // Only generate shield if king is on lower ranks (where shield makes sense)
            if (rank < 6) // King on ranks 0-5
            {
                // Add squares from adjacent files and king file, ranks +1, +2, +3
                for (int r = rank + 1; r <= Math.Min(7, rank + 3); r++)
                {
                    // Left file (if exists)
                    if (file > 0)
                        shieldSquares.Add((byte)(r * 8 + file - 1));

                    // King file
                    shieldSquares.Add((byte)(r * 8 + file));

                    // Right file (if exists)
                    if (file < 7)
                        shieldSquares.Add((byte)(r * 8 + file + 1));
                }
            }

            // Create mask from shield squares
            BitBoard mask = new BitBoard(0);
            foreach (var sq in shieldSquares)
            {
                mask = mask.Set(sq);
            }
            _whiteKingShieldMask[kingPos] = mask;

            var maxP = mask.ExtractBits(mask) + 1;

            // Generate all 512 possible pawn patterns
            for (int pattern = 0; pattern < maxP; pattern++)
            {
                // Reconstruct pawn bitboard from pattern
                BitBoard pawns = new BitBoard(0);
                for (int i = 0; i < shieldSquares.Count && i < 9; i++)
                {
                    if ((pattern & (1 << i)) != 0)
                    {
                        pawns = pawns.Set(shieldSquares[i]);
                    }
                }

                // Calculate score using current evaluation logic
                // Only evaluate ranks 2 and 3 (not rank 4) based on analysis
                short score = (short)WhiteKingShield(kingPos, pawns);

                var p = pawns.ExtractBits(mask);

                _whiteKingShieldLookup[kingPos * 512 + p] = score;
            }
        }

        #endregion
    }
}
