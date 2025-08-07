using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Boards;

public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPinsEnd(byte coordinate) => GetWhiteBishopDiscoveredCheck(coordinate)
             + GetWhiteBishopAbsolutePin(coordinate)
             + GetWhiteBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPinsOpening(byte coordinate) => GetWhiteBishopDiscoveredCheck(coordinate)
                 + GetWhiteBishopDiscoveredAttack(coordinate)
                 + GetWhiteBishopAbsolutePin(coordinate)
                 + GetWhiteBishopPartialPin(coordinate)
                 + GetWhiteBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetWhiteMovablePawns() => ((_boards[Pieces.WhitePawn] << 8) & _empty) >> 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetBlackMovablePawns() => ((_boards[Pieces.BlackPawn] >> 8) & _empty) << 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackRook] | _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopAbsolutePin(byte coordinate)
    {
        if (!_whiteBishopPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredCheck(byte coordinate)
    {
        if (!_whiteBishopPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteRook] | _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopAbsolutePin(byte coordinate)
    {
        if (!_blackBishopPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any())
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredCheck(byte coordinate)
    {
        if (!_blackBishopPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopPinsOpening(byte coordinate) => GetBlackBishopDiscoveredCheck(coordinate)
               + GetBlackBishopDiscoveredAttack(coordinate)
               + GetBlackBishopAbsolutePin(coordinate)
               + GetBlackBishopPartialPin(coordinate)
               + GetBlackBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopPinsEnd(byte coordinate) => GetBlackBishopDiscoveredCheck(coordinate)
                 + GetBlackBishopAbsolutePin(coordinate)
                 + GetBlackBishopBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopBattary(byte coordinate)
    {
        if (_boards[Pieces.BlackQueen].IsZero()) return 0;

        var pattern = _blackBishopPatterns[coordinate] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.WhiteQueen];
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookAbsolutePin(byte coordinate)
    {
        if (!_blackRookPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredCheck(byte coordinate)
    {
        if (!_blackRookPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookBattary(byte coordinate)
    {
        var pattern = _blackRookPatterns[coordinate] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.BlackQueen].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.BlackQueen]) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (_boards[Pieces.BlackRook].Count() > 1 && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();


        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookPinsOpening(byte coordinate) => GetBlackRookDiscoveredCheck(coordinate)
                     + GetBlackRookDiscoveredAttack(coordinate)
                     + GetBlackRookAbsolutePin(coordinate)
                     + GetBlackRookPartialPin(coordinate)
                     + GetBlackRookBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookPinsEnd(byte coordinate) => GetBlackRookDiscoveredCheck(coordinate)
                 + GetBlackRookAbsolutePin(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenPins(byte coordinate) => GetBlackQueenDiscoveredCheck(coordinate)
                 + GetBlackQueenAbsolutePin(coordinate)
                 + GetBlackQueenBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenBattary(byte coordinate)
    {
        var pattern = _blackQueenPatterns[coordinate] & _whiteKingPatterns[_boards[Pieces.WhiteKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.BlackRook].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.BlackRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (_boards[Pieces.BlackBishop].Any() && (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.BlackBishop]) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenAbsolutePin(byte coordinate)
    {
        if (!_blackQueenPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenDiscoveredCheck(byte coordinate)
    {
        if (!_blackQueenPatterns[coordinate].IsSet(_boards[Pieces.WhiteKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook] | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.WhiteKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopBattary(byte coordinate)
    {
        if (_boards[Pieces.WhiteQueen].IsZero()) return 0;

        var pattern = _whiteBishopPatterns[coordinate] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPinsEnd(byte coordinate) => GetWhiteRookDiscoveredCheck(coordinate)
                 + GetWhiteRookAbsolutePin(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPinsOpening(byte coordinate) => GetWhiteRookDiscoveredCheck(coordinate)
                 + GetWhiteRookDiscoveredAttack(coordinate)
                 + GetWhiteRookAbsolutePin(coordinate)
                 + GetWhiteRookPartialPin(coordinate)
                 + GetWhiteRookBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookBattary(byte coordinate)
    {
        var pattern = _whiteRookPatterns[coordinate] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.WhiteQueen].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.WhiteQueen]) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (_boards[Pieces.WhiteRook].Count() > 1 && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPartialPin(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredAttack(byte coordinate)
    {
        BitBoard bit = _boards[Pieces.BlackQueen];
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookAbsolutePin(byte coordinate)
    {
        if (!_whiteRookPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredCheck(byte coordinate)
    {
        if (!_whiteRookPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenPins(byte coordinate) => GetWhiteQueenDiscoveredCheck(coordinate)
                 + GetWhiteQueenAbsolutePin(coordinate)
                 + GetWhiteQueenBattary(coordinate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenBattary(byte coordinate)
    {
        var pattern = _whiteQueenPatterns[coordinate] & _blackKingPatterns[_boards[Pieces.BlackKing].BitScanForward()];

        if (pattern.IsZero()) return 0;

        if (_boards[Pieces.WhiteRook].Any() && (coordinate.XrayRookAttacks(_occupied, _boards[Pieces.WhiteRook]) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (_boards[Pieces.WhiteBishop].Any() && (coordinate.XrayBishopAttacks(_occupied, _boards[Pieces.WhiteBishop]) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenAbsolutePin(byte coordinate)
    {
        if (!_whiteQueenPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackRook];

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = _boards[Pieces.BlackKnight] | _boards[Pieces.BlackBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenDiscoveredCheck(byte coordinate)
    {
        if (!_whiteQueenPatterns[coordinate].IsSet(_boards[Pieces.BlackKing]))
            return 0;

        var blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteRook] | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = _boards[Pieces.WhiteKnight] | _boards[Pieces.WhiteBishop];

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & _boards[Pieces.BlackKing]).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        return _evaluationService.GetRentgenValue();
    }
}
