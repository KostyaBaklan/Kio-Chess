using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    public partial class Board
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhitePawnValue()
        {
            if (_boards[Pieces.WhitePawn].IsZero())
                return _evaluationService.GetNoPawnsValue();

            int value = 0;

            var bits = _boards[Pieces.WhitePawn];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetWhitePawnFullValue(coordinate);

                if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_whiteDoublePawns[coordinate] & _boards[Pieces.WhitePawn]).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_whiteFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()
                    && (_whitePassedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
                {
                    var pp = _evaluationService.GetWhitePassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;
                        //if ((_whiteCandidatePawnsAttackBack[coordinate] & _boards[Pieces.WhitePawn]).Any())
                        //{
                        //    value += _evaluationService.GetProtectedPassedPawnValue();
                        //}
                    }
                }


                if ((_whiteIsolatedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else
                {
                    for (byte c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
                    {
                        if ((_whiteBackwardPawns[coordinate][c].Key & _boards[Pieces.WhitePawn]).IsZero() &&
                            (_whiteBackwardPawns[coordinate][c].Value & _boards[Pieces.BlackPawn]).Any())
                        {
                            value -= _evaluationService.GetBackwardPawnValue();
                            break;
                        }
                    }
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackPawnValue()
        {
            if (_boards[Pieces.BlackPawn].IsZero()) return _evaluationService.GetNoPawnsValue();

            int value = 0;
            var bits = _boards[Pieces.BlackPawn];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackPawnFullValue(coordinate);
                if ((_blackBlockedPawns[coordinate] & _whites).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_blackDoublePawns[coordinate] & _boards[Pieces.BlackPawn]).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_blackFacing[coordinate] & (_boards[Pieces.WhitePawn] | _boards[Pieces.BlackPawn])).IsZero()
                    && (_blackPassedPawns[coordinate] & _boards[Pieces.WhitePawn]).IsZero())
                {
                    var pp = _evaluationService.GetBlackPassedPawnValue(coordinate);
                    if (pp > 0)
                    {
                        value += pp;
                        //if ((_blackCandidatePawnsAttackBack[coordinate] & _boards[Pieces.BlackPawn]).Any())
                        //{
                        //    value += _evaluationService.GetProtectedPassedPawnValue();
                        //}
                    }
                }


                if ((_blackIsolatedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else
                {
                    for (byte c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
                    {
                        if ((_blackBackwardPawns[coordinate][c].Key & _boards[Pieces.BlackPawn]).IsZero() &&
                            (_blackBackwardPawns[coordinate][c].Value & _boards[Pieces.WhitePawn]).Any())
                        {
                            value -= _evaluationService.GetBackwardPawnValue();
                            break;
                        }
                    }
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackPawnOpening()
        {
            int value = 0;
            var bits = _boards[Pieces.BlackPawn];
            while (bits.Any())
            {
                var coordinate = bits.BitScanForward();
                value += _evaluationService.GetBlackPawnFullValue(coordinate);
                if ((_blackBlockedPawns[coordinate] & _whites).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_blackDoublePawns[coordinate] & _boards[Pieces.BlackPawn]).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if ((_blackIsolatedPawns[coordinate] & _boards[Pieces.BlackPawn]).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }
                else
                {
                    for (byte c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
                    {
                        if ((_blackBackwardPawns[coordinate][c].Key & _boards[Pieces.BlackPawn]).IsZero() &&
                            (_blackBackwardPawns[coordinate][c].Value & _boards[Pieces.WhitePawn]).Any())
                        {
                            value -= _evaluationService.GetBackwardPawnValue();
                            break;
                        }
                    }
                }
                bits = bits.Remove(coordinate);
            }

            return value;
        }
    }
}
