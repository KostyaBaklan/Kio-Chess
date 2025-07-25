using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenMobility(byte to) => (to.QueenAttacks(_occupied) & (_empty.Remove(_whitePawnAttacks)
            | _whiteKingZone)).Count() *
        _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookMobility(byte to) => (to.RookAttacks(_occupied) & (_empty.Remove(_whitePawnAttacks)
            | _whiteKingZone)).Count() *
       _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopMobility(byte to) => (to.BishopAttacks(_occupied) & (_empty.Remove(_whitePawnAttacks) | _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteKnight]
            | _whiteKingZone))
            .Count() * _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackKnightMobility(byte to) => (_blackKnightPatterns[to] & (_empty.Remove(_whitePawnAttacks) | _boards[Pieces.WhiteQueen] | _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteBishop]
            | _whiteKingZone))
            .Count() * _evaluationService.GetKnightMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenMobility(byte to) => (to.QueenAttacks(_occupied) & (_empty.Remove(_blackPawnAttacks)
            | _blackKingZone)).Count() *
        _evaluationService.GetQueenMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookMobility(byte to) => (to.RookAttacks(_occupied) & (_empty.Remove(_blackPawnAttacks)
            | _blackKingZone)).Count() *
        _evaluationService.GetRookMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopMobility(byte to) => (to.BishopAttacks(_occupied) & (_empty.Remove(_blackPawnAttacks) | _boards[Pieces.BlackRook] | _boards[Pieces.BlackKnight]
            | _blackKingZone)).Count() *
        _evaluationService.GetBishopMobilityValue();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteKnightMobility(byte to) => (_whiteKnightPatterns[to]
            & (_empty.Remove(_blackPawnAttacks) | _boards[Pieces.BlackQueen] | _boards[Pieces.BlackRook] | _boards[Pieces.BlackBishop]
            | _blackKingZone)).Count()
            * _evaluationService.GetKnightMobilityValue();
}
