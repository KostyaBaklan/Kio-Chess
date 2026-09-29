using Engine.Interfaces.Config;
using Engine.Models.Boards.Buffers;
using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Services.Evaluation
{ 

    public class PstService
    {
        private readonly int _middleThreshold;
        private readonly int _endThreshold;

        // in ctor:
        
        private readonly PieceBuffer<PstTable> _pst;

        public PstService(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        {
            var boardStateConfiguration = configuration.GeneralConfiguration.BoardState;
            _middleThreshold = boardStateConfiguration.FullMiddleThreshold;
            _endThreshold = boardStateConfiguration.FullEndThreshold;
            var evaluation = configuration.Evaluation;

            var pieceValues = evaluation.PieceValues;

            _pst = new PieceBuffer<PstTable>();

            _pst[Pieces.WhitePawn] = GenerateTable(staticValueProvider, Pieces.WhitePawn, pieceValues[0]);
            _pst[Pieces.WhiteKnight] = GenerateTable(staticValueProvider, Pieces.WhiteKnight, pieceValues[1]);
            _pst[Pieces.WhiteBishop] = GenerateTable(staticValueProvider, Pieces.WhiteBishop, pieceValues[2]);
            _pst[Pieces.WhiteRook] = GenerateTable(staticValueProvider, Pieces.WhiteRook, pieceValues[3]);
            _pst[Pieces.WhiteQueen] = GenerateTable(staticValueProvider, Pieces.WhiteQueen, pieceValues[4]);
            _pst[Pieces.WhiteKing] = GenerateTable(staticValueProvider, Pieces.WhiteKing, pieceValues[5]);
            _pst[Pieces.BlackPawn] = GenerateTable(staticValueProvider, Pieces.BlackPawn, pieceValues[0]);
            _pst[Pieces.BlackKnight] = GenerateTable(staticValueProvider, Pieces.BlackKnight, pieceValues[1]);
            _pst[Pieces.BlackBishop] = GenerateTable(staticValueProvider, Pieces.BlackBishop, pieceValues[2]);
            _pst[Pieces.BlackRook] = GenerateTable(staticValueProvider, Pieces.BlackRook, pieceValues[3]);
            _pst[Pieces.BlackQueen] = GenerateTable(staticValueProvider, Pieces.BlackQueen, pieceValues[4]);
            _pst[Pieces.BlackKing] = GenerateTable(staticValueProvider, Pieces.BlackKing, pieceValues[5]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetPstValue(byte piece, byte position, byte phase) => _pst[piece][phase][position];

        private PstTable GenerateTable( IStaticValueProvider staticValueProvider, byte piece, int pieceValue)
        {
            var table = new PstTable();

            for (byte cell = 0; cell < 64; cell++)
            {
                int openingValue = staticValueProvider.GetValue(piece, Phase.Opening, cell);
                int middleValue = staticValueProvider.GetValue(piece, Phase.Middle, cell);
                int endValue = staticValueProvider.GetValue(piece, Phase.End, cell);

                for (int p = 0; p < 25; p++)
                {
                    double value;

                    if (p > _middleThreshold)
                    {
                        // Opening <-> Middle band, remapped to 0..1
                        double t = (double)(p - _middleThreshold) / (24 - _middleThreshold);
                        value = openingValue * t + middleValue * (1.0 - t);
                    }
                    else if (p < _endThreshold)
                    {
                        value = endValue;
                    }
                    else
                    {
                        // Middle <-> End band, remapped to 0..1
                        double t = (double)(p - _endThreshold) / (_middleThreshold - _endThreshold);
                        value = middleValue * t + endValue * (1.0 - t);
                    }

                    table[p][cell] = (short)(Round(value) + pieceValue);
                }
            }

            return table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Round(double value) => (int)(Math.Round(value / 5.0, MidpointRounding.AwayFromZero) * 5.0);
    }
}
