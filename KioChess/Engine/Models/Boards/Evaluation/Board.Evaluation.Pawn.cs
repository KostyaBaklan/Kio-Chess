using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhitePawnOpening()
        {
            int value = 0;
            ref var boardBase = ref _boards[0];

            var bits = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            BitBoard whites = bits;
            BitBoard blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhitePawnFullValue(coordinate);

                if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_whiteDoublePawns[coordinate] & whites).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }


                if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhitePawnMiddle()
        {
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            if (bits.IsZero())
                return _evaluationService.GetNoPawnsValue();

            int value = 0;
            BitBoard whites = bits;
            BitBoard blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            BitBoard allPawns = whites | blacks;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhitePawnFullValue(coordinate);

                if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_whiteDoublePawns[coordinate] & whites).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_whiteFacing[coordinate] & allPawns).IsZero()
                    && (_whitePassedPawns[coordinate] & blacks).IsZero())
                {
                    var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;

                        if ((_whiteProtectedPassedPawns[coordinate] & whites).Any())
                        {
                            value += _evaluationService.GetWhiteProtectedPassedPawnValue(coordinate);
                        }

                        if ((_whiteConnectedPassedPawns[coordinate] & whites).Any())
                        {
                            value += _evaluationService.GetWhiteConnectedPassedPawnValue(coordinate);
                        }

                        //// Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                        //BitBoard rookFile = _rookFiles[coordinate];
                        //BitBoard friendlyRooksOnFile = rookFile & _boards[Pieces.WhiteRook];
                        //if (friendlyRooksOnFile.Any())
                        //{
                        //    // Check if any rook is behind (lower square for white)
                        //    var rookSquare = friendlyRooksOnFile.BitScanForward();
                        //    if (rookSquare < coordinate)
                        //    {
                        //        // Check no pieces between rook and pawn using lookup table
                        //        if ((_fileBetween[rookSquare][coordinate] & _occupied).IsZero())
                        //        {
                        //            value += _evaluationService.GetRookBehindPassedPawnValue();
                        //        }
                        //    }
                        //}
                    }
                }


                if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhitePawnEnd()
        {
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            if (bits.IsZero())
                return _evaluationService.GetNoPawnsValue();

            int value = 0;
            BitBoard whites = bits;
            BitBoard blacks = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            BitBoard allPawns = whites | blacks;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhitePawnFullValue(coordinate);

                if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_whiteDoublePawns[coordinate] & whites).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_whiteFacing[coordinate] & allPawns).IsZero()
                    && (_whitePassedPawns[coordinate] & blacks).IsZero())
                {
                    var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;

                        if ((_whiteProtectedPassedPawns[coordinate] & whites).Any())
                        {
                            value += _evaluationService.GetWhiteProtectedPassedPawnValue(coordinate);
                        }

                        if ((_whiteConnectedPassedPawns[coordinate] & whites).Any())
                        {
                            value += _evaluationService.GetWhiteConnectedPassedPawnValue(coordinate);
                        }

                        // Full king distance factor in endgame
                        value += _evaluationService.GetKingDistanceFactor(coordinate, _whiteKingPosition, _blackKingPosition);

                        // Check for blockade on the next square
                        byte nextSquare = (byte)(coordinate + 8);
                        if (_blacks.IsSet(nextSquare))
                        {
                            value -= _evaluationService.GetBlockadePenalty(_pieces[nextSquare]);
                        }

                        // Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                        BitBoard friendlyRooksOnFile = _rookFiles[coordinate] & Unsafe.Add(ref boardBase, Pieces.WhiteRook);
                        if (friendlyRooksOnFile.Any())
                        {
                            // Check if any rook is behind (lower square for white)
                            var rookSquare = friendlyRooksOnFile.BitScanForward();
                            if (rookSquare < coordinate && (_fileBetween[rookSquare][coordinate] & _occupied).IsZero())
                            {
                                value += _evaluationService.GetRookBehindPassedPawnValue();
                            }
                        }

                        // Unstoppable passed pawn: Check if enemy king is outside the "square of the pawn"
                        // Only check if path ahead is clear (no blockade)
                        if ((_whiteFacing[coordinate] & _occupied).IsZero() && !_whitePassedPawnSquare[coordinate].IsSet(_blackKingPosition))
                        {
                            value += _evaluationService.GetUnstoppablePassedPawnValue();
                        }

                        //// Outside passed pawn bonus: pawns on files A, B, G, H are more valuable in endgame
                        //// because they divert the enemy king, allowing friendly king to penetrate
                        //if (_outsideFiles.IsSet(coordinate))
                        //{
                        //    value += _evaluationService.GetOutsidePassedPawnValue();
                        //}
                    }
                }


                if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_whiteBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteBackwardAttackPawns[coordinate] & blacks).Any()) || (coordinate < Squares.A3 && (_whiteStartBackwardSupportPawns[coordinate] & whites).IsZero() &&
                            (_whiteStartBackwardAttackPawns[coordinate] & blacks).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackPawnOpening()
        {
            int value = 0;
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            BitBoard blacks = bits;
            BitBoard whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackPawnFullValue(coordinate);
                if ((_blackBlockedPawns[coordinate] & _whites).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_blackDoublePawns[coordinate] & blacks).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackPawnMiddle()
        {
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            if (bits.IsZero()) return _evaluationService.GetNoPawnsValue();

            int value = 0;
            BitBoard blacks = bits;
            BitBoard whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            BitBoard allPawns = whites | blacks;
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackPawnFullValue(coordinate);
                if ((_blackBlockedPawns[coordinate] & _whites).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_blackDoublePawns[coordinate] & blacks).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_blackFacing[coordinate] & allPawns).IsZero()
                    && (_blackPassedPawns[coordinate] & whites).IsZero())
                {
                    var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;

                        if ((_blackProtectedPassedPawns[coordinate] & blacks).Any())
                        {
                            value += _evaluationService.GetBlackProtectedPassedPawnValue(coordinate);
                        }

                        if ((_blackConnectedPassedPawns[coordinate] & blacks).Any())
                        {
                            value += _evaluationService.GetBlackConnectedPassedPawnValue(coordinate);
                        }

                        //// Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                        //BitBoard rookFile = _rookFiles[coordinate];
                        //BitBoard friendlyRooksOnFile = rookFile & _boards[Pieces.BlackRook];
                        //if (friendlyRooksOnFile.Any())
                        //{
                        //    // Check if any rook is behind (higher square for black)
                        //    var rookSquare = friendlyRooksOnFile.BitScanReverse();
                        //    if (rookSquare > coordinate)
                        //    {
                        //        // Check no pieces between rook and pawn using lookup table
                        //        if ((_fileBetween[coordinate][rookSquare] & _occupied).IsZero())
                        //        {
                        //            value += _evaluationService.GetRookBehindPassedPawnValue();
                        //        }
                        //    }
                        //}
                    }
                }


                if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackPawnEnd()
        {
            ref var boardBase = ref _boards[0];
            var bits = Unsafe.Add(ref boardBase, Pieces.BlackPawn);
            if (bits.IsZero()) return _evaluationService.GetNoPawnsValue();

            int value = 0;
            BitBoard blacks = bits;
            BitBoard whites = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
            BitBoard allPawns = whites | blacks;

            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackPawnFullValue(coordinate);

                if ((_blackBlockedPawns[coordinate] & _whites).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_blackDoublePawns[coordinate] & blacks).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_blackFacing[coordinate] & allPawns).IsZero()
                    && (_blackPassedPawns[coordinate] & whites).IsZero())
                {
                    var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;

                        if ((_blackProtectedPassedPawns[coordinate] & blacks).Any())
                        {
                            value += _evaluationService.GetBlackProtectedPassedPawnValue(coordinate);
                        }

                        if ((_blackConnectedPassedPawns[coordinate] & blacks).Any())
                        {
                            value += _evaluationService.GetBlackConnectedPassedPawnValue(coordinate);
                        }

                        // Full king distance factor in endgame
                        value += _evaluationService.GetKingDistanceFactor(coordinate, _blackKingPosition, _whiteKingPosition);

                        // Check for blockade on the next square
                        byte nextSquare = (byte)(coordinate - 8);
                        if (_whites.IsSet(nextSquare))
                        {
                            value -= _evaluationService.GetBlockadePenalty(_pieces[nextSquare]);
                        }

                        // Tarrasch Rule: Check if any friendly rook is behind this passed pawn
                        BitBoard friendlyRooksOnFile = _rookFiles[coordinate] & Unsafe.Add(ref boardBase, Pieces.BlackRook);
                        if (friendlyRooksOnFile.Any())
                        {
                            // Check if any rook is behind (higher square for black)
                            var rookSquare = friendlyRooksOnFile.BitScanReverse();
                            if (rookSquare > coordinate && (_fileBetween[coordinate][rookSquare] & _occupied).IsZero())
                            {
                                value += _evaluationService.GetRookBehindPassedPawnValue();
                            }
                        }

                        // Unstoppable passed pawn: Check if enemy king is outside the "square of the pawn"
                        // Only check if path ahead is clear (no blockade)
                        if ((_blackFacing[coordinate] & _occupied).IsZero() && !_blackPassedPawnSquare[coordinate].IsSet(_whiteKingPosition))
                        {
                            value += _evaluationService.GetUnstoppablePassedPawnValue();
                        }

                        //// Outside passed pawn bonus: pawns on files A, B, G, H are more valuable in endgame
                        //// because they divert the enemy king, allowing friendly king to penetrate
                        //if (_outsideFiles.IsSet(coordinate))
                        //{
                        //    value += _evaluationService.GetOutsidePassedPawnValue();
                        //}
                    }
                }


                if ((_blackIsolatedPawns[coordinate] & blacks).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else if (((_blackBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackBackwardAttackPawns[coordinate] & whites).Any()) || (coordinate > Squares.H6 && (_blackStartBackwardSupportPawns[coordinate] & blacks).IsZero() &&
                            (_blackStartBackwardAttackPawns[coordinate] & whites).Any()))
                {
                    value -= _evaluationService.GetBackwardPawnValue();
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
