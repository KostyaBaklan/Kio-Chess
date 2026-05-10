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
    private BitBoard GetWhiteMovablePawns()
    {
        ref var boardBase = ref _boards[0];
        return ((Unsafe.Add(ref boardBase, Pieces.WhitePawn) << 8) & _empty) >> 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard GetBlackMovablePawns()
    {
        ref var boardBase = ref _boards[0];
        return ((Unsafe.Add(ref boardBase, Pieces.BlackPawn) >> 8) & _empty) << 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredAttack(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteRook) | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopPartialPin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.BlackRook) | Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        if (bit.IsZero() || !_whiteBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackRook);

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopAbsolutePin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_whiteBishopPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.BlackKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackRook);

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopDiscoveredCheck(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_whiteBishopPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.BlackKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteRook) | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopPartialPin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteRook);

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredAttack(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.WhiteRook) | Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        if (bit.IsZero() || !_blackBishopPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackRook) | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopAbsolutePin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_blackBishopPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.WhiteKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteRook);

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any())
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackBishopDiscoveredCheck(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_blackBishopPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.WhiteKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackRook) | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()) //Discovered Check
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
        ref var boardBase = ref _boards[0];
        if (Unsafe.Add(ref boardBase, Pieces.BlackQueen).IsZero()) return 0;

        var pattern = _blackBishopPatterns[coordinate] & _whiteKingAttacks;

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.BlackQueen)) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookPartialPin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredAttack(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.WhiteQueen);
        if (bit.IsZero() || !_blackRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered Attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookAbsolutePin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_blackRookPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.WhiteKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookDiscoveredCheck(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_blackRookPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.WhiteKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()) //Discovered Check
            return _evaluationService.GetDiscoveredCheckValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackRookBattary(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        var pattern = _blackRookPatterns[coordinate] & _whiteKingAttacks;

        if (pattern.IsZero()) return 0;

        if (Unsafe.Add(ref boardBase, Pieces.BlackQueen).Any() && (coordinate.XrayRookAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.BlackQueen)) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (Unsafe.Add(ref boardBase, Pieces.BlackRook).Count() > 1 && (coordinate.XrayRookAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.BlackRook)) & pattern).Any())
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
        ref var boardBase = ref _boards[0];
        var pattern = _blackQueenPatterns[coordinate] & _whiteKingAttacks;

        if (pattern.IsZero()) return 0;

        if (Unsafe.Add(ref boardBase, Pieces.BlackRook).Any() && (coordinate.XrayRookAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.BlackRook)) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (Unsafe.Add(ref boardBase, Pieces.BlackBishop).Any() && (coordinate.XrayBishopAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.BlackBishop)) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenAbsolutePin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_blackQueenPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.WhiteKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteRook);

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop);

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlackQueenDiscoveredCheck(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_blackQueenPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.WhiteKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackRook) | GetBlackMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackBishop);

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.WhiteKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteBishopBattary(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (Unsafe.Add(ref boardBase, Pieces.WhiteQueen).IsZero()) return 0;

        var pattern = _whiteBishopPatterns[coordinate] & _blackKingAttacks;

        if (pattern.IsZero()) return 0;

        return (coordinate.XrayBishopAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.WhiteQueen)) & pattern).Any() ? _evaluationService.GetQueenBattaryValue() : 0;
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
        ref var boardBase = ref _boards[0];
        var pattern = _whiteRookPatterns[coordinate] & _blackKingAttacks;

        if (pattern.IsZero()) return 0;

        if (Unsafe.Add(ref boardBase, Pieces.WhiteQueen).Any() && (coordinate.XrayRookAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.WhiteQueen)) & pattern).Any())
            return _evaluationService.GetQueenBattaryValue();

        if (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Count() > 1 && (coordinate.XrayRookAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.WhiteRook)) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookPartialPin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetPartialPinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredAttack(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        BitBoard bit = Unsafe.Add(ref boardBase, Pieces.BlackQueen);
        if (bit.IsZero() || !_whiteRookPatterns[coordinate].IsSet(bit))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & bit).Any()) //Discovered attack
            return _evaluationService.GetDiscoveredAttackValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookAbsolutePin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_whiteRookPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.BlackKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
            return _evaluationService.GetAbsolutePinValue();
        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteRookDiscoveredCheck(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_whiteRookPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.BlackKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop);

        var attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
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
        ref var boardBase = ref _boards[0];
        var pattern = _whiteQueenPatterns[coordinate] & _blackKingAttacks;

        if (pattern.IsZero()) return 0;

        if (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Any() && (coordinate.XrayRookAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.WhiteRook)) & pattern).Any())
            return _evaluationService.GetRookBattaryValue();

        if (Unsafe.Add(ref boardBase, Pieces.WhiteBishop).Any() && (coordinate.XrayBishopAttacks(_occupied, Unsafe.Add(ref boardBase, Pieces.WhiteBishop)) & pattern).Any())
            return _evaluationService.GetBishopBattaryValue();

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenAbsolutePin(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_whiteQueenPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.BlackKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackRook);

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        blocker = Unsafe.Add(ref boardBase, Pieces.BlackKnight) | Unsafe.Add(ref boardBase, Pieces.BlackBishop);

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetAbsolutePinValue();
        }

        return _evaluationService.GetRentgenValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetWhiteQueenDiscoveredCheck(byte coordinate)
    {
        ref var boardBase = ref _boards[0];
        if (!_whiteQueenPatterns[coordinate].IsSet(Unsafe.Add(ref boardBase, Pieces.BlackKing)))
            return 0;

        var blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteRook) | GetWhiteMovablePawns();

        var attacks = coordinate.XrayBishopAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        blocker = Unsafe.Add(ref boardBase, Pieces.WhiteKnight) | Unsafe.Add(ref boardBase, Pieces.WhiteBishop);

        attacks = coordinate.XrayRookAttacks(_occupied, blocker);

        if ((attacks & Unsafe.Add(ref boardBase, Pieces.BlackKing)).Any()) //Discovered Check
        {
            return _evaluationService.GetDiscoveredCheckValue();
        }

        return _evaluationService.GetRentgenValue();
    }
}
