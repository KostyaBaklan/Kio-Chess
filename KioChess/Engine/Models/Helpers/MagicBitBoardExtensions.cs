﻿/*
*Copyright (C) 2007 Pradyumna Kannan.
*
*This code is provided 'as-is', without any expressed or implied warranty.
*In no event will the authors be held liable for any damages arising from
*the use of this code. Permission is granted to anyone to use this
*code for any purpose, including commercial applications, and to alter
*it and redistribute it freely, subject to the following restrictions:
*
*1. The origin of this code must not be misrepresented; you must not
*claim that you wrote the original code. If you use this code in a
*product, an acknowledgment in the product documentation would be
*appreciated but is not required.
*
*2. Altered source versions must be plainly marked as such, and must not be
*misrepresented as being the original code.
*
*3. This notice may not be removed or altered from any source distribution.
*
* -------------------------------------------------------------------------
* Converted from C to C# code by Rudy Alex Kohn, 2017 - for use in MUCI.
* The same conditions as described above is *STILL* in effect.
*
* Original files: magicmoves.c and magicmoves.h
* Conditional compile paths were removed.
* The original algorithm and data were not changed in any way.
*
* WHATEVER YOU DO, DO NOT ALTER ANYTHING IN HERE!
*
*/

using System.Runtime.CompilerServices;
using Engine.Models.Boards;

namespace Engine.Models.Helpers;


// ReSharper disable once InconsistentNaming
public static class MagicBitBoardExtensions
{
    private const ulong One = 1UL;

    private const ulong _ff = 0xFFUL;

    private const int _magicBishopDbLength = 512;

    private const int _magicRookDbLength = 4096;

    private static readonly BitBoard[][] _magicBishopDb = new BitBoard[64][];

    private static readonly ulong[] _magicMovesBMagics =
        {
            0x0002020202020200UL, 0x0002020202020000UL, 0x0004010202000000UL, 0x0004040080000000UL, 0x0001104000000000UL, 0x0000821040000000UL, 0x0000410410400000UL, 0x0000104104104000UL,
            0x0000040404040400UL, 0x0000020202020200UL, 0x0000040102020000UL, 0x0000040400800000UL, 0x0000011040000000UL, 0x0000008210400000UL, 0x0000004104104000UL, 0x0000002082082000UL,
            0x0004000808080800UL, 0x0002000404040400UL, 0x0001000202020200UL, 0x0000800802004000UL, 0x0000800400A00000UL, 0x0000200100884000UL, 0x0000400082082000UL, 0x0000200041041000UL,
            0x0002080010101000UL, 0x0001040008080800UL, 0x0000208004010400UL, 0x0000404004010200UL, 0x0000840000802000UL, 0x0000404002011000UL, 0x0000808001041000UL, 0x0000404000820800UL,
            0x0001041000202000UL, 0x0000820800101000UL, 0x0000104400080800UL, 0x0000020080080080UL, 0x0000404040040100UL, 0x0000808100020100UL, 0x0001010100020800UL, 0x0000808080010400UL,
            0x0000820820004000UL, 0x0000410410002000UL, 0x0000082088001000UL, 0x0000002011000800UL, 0x0000080100400400UL, 0x0001010101000200UL, 0x0002020202000400UL, 0x0001010101000200UL,
            0x0000410410400000UL, 0x0000208208200000UL, 0x0000002084100000UL, 0x0000000020880000UL, 0x0000001002020000UL, 0x0000040408020000UL, 0x0004040404040000UL, 0x0002020202020000UL,
            0x0000104104104000UL, 0x0000002082082000UL, 0x0000000020841000UL, 0x0000000000208800UL, 0x0000000010020200UL, 0x0000000404080200UL, 0x0000040404040400UL, 0x0002020202020200UL
        };

    private static readonly ulong[] _magicMovesBMask =
        {
            0x0040201008040200UL, 0x0000402010080400UL, 0x0000004020100A00UL, 0x0000000040221400UL, 0x0000000002442800UL, 0x0000000204085000UL, 0x0000020408102000UL, 0x0002040810204000UL,
            0x0020100804020000UL, 0x0040201008040000UL, 0x00004020100A0000UL, 0x0000004022140000UL, 0x0000000244280000UL, 0x0000020408500000UL, 0x0002040810200000UL, 0x0004081020400000UL,
            0x0010080402000200UL, 0x0020100804000400UL, 0x004020100A000A00UL, 0x0000402214001400UL, 0x0000024428002800UL, 0x0002040850005000UL, 0x0004081020002000UL, 0x0008102040004000UL,
            0x0008040200020400UL, 0x0010080400040800UL, 0x0020100A000A1000UL, 0x0040221400142200UL, 0x0002442800284400UL, 0x0004085000500800UL, 0x0008102000201000UL, 0x0010204000402000UL,
            0x0004020002040800UL, 0x0008040004081000UL, 0x00100A000A102000UL, 0x0022140014224000UL, 0x0044280028440200UL, 0x0008500050080400UL, 0x0010200020100800UL, 0x0020400040201000UL,
            0x0002000204081000UL, 0x0004000408102000UL, 0x000A000A10204000UL, 0x0014001422400000UL, 0x0028002844020000UL, 0x0050005008040200UL, 0x0020002010080400UL, 0x0040004020100800UL,
            0x0000020408102000UL, 0x0000040810204000UL, 0x00000A1020400000UL, 0x0000142240000000UL, 0x0000284402000000UL, 0x0000500804020000UL, 0x0000201008040200UL, 0x0000402010080400UL,
            0x0002040810204000UL, 0x0004081020400000UL, 0x000A102040000000UL, 0x0014224000000000UL, 0x0028440200000000UL, 0x0050080402000000UL, 0x0020100804020000UL, 0x0040201008040200UL
        };

    private static readonly BitBoard[][] _magicRookDb = new BitBoard[64][];

    private static readonly ulong[] _magicmovesRMagics =
        {
            0x0080001020400080UL, 0x0040001000200040UL, 0x0080081000200080UL, 0x0080040800100080UL, 0x0080020400080080UL, 0x0080010200040080UL, 0x0080008001000200UL, 0x0080002040800100UL,
            0x0000800020400080UL, 0x0000400020005000UL, 0x0000801000200080UL, 0x0000800800100080UL, 0x0000800400080080UL, 0x0000800200040080UL, 0x0000800100020080UL, 0x0000800040800100UL,
            0x0000208000400080UL, 0x0000404000201000UL, 0x0000808010002000UL, 0x0000808008001000UL, 0x0000808004000800UL, 0x0000808002000400UL, 0x0000010100020004UL, 0x0000020000408104UL,
            0x0000208080004000UL, 0x0000200040005000UL, 0x0000100080200080UL, 0x0000080080100080UL, 0x0000040080080080UL, 0x0000020080040080UL, 0x0000010080800200UL, 0x0000800080004100UL,
            0x0000204000800080UL, 0x0000200040401000UL, 0x0000100080802000UL, 0x0000080080801000UL, 0x0000040080800800UL, 0x0000020080800400UL, 0x0000020001010004UL, 0x0000800040800100UL,
            0x0000204000808000UL, 0x0000200040008080UL, 0x0000100020008080UL, 0x0000080010008080UL, 0x0000040008008080UL, 0x0000020004008080UL, 0x0000010002008080UL, 0x0000004081020004UL,
            0x0000204000800080UL, 0x0000200040008080UL, 0x0000100020008080UL, 0x0000080010008080UL, 0x0000040008008080UL, 0x0000020004008080UL, 0x0000800100020080UL, 0x0000800041000080UL,
            0x0000102040800101UL, 0x0000102040008101UL, 0x0000081020004101UL, 0x0000040810002101UL, 0x0001000204080011UL, 0x0001000204000801UL, 0x0001000082000401UL, 0x0000002040810402UL
        };

    private static readonly ulong[] _magicmovesRMask = {
            0x000101010101017EUL, 0x000202020202027CUL, 0x000404040404047AUL, 0x0008080808080876UL, 0x001010101010106EUL, 0x002020202020205EUL, 0x004040404040403EUL, 0x008080808080807EUL,
            0x0001010101017E00UL, 0x0002020202027C00UL, 0x0004040404047A00UL, 0x0008080808087600UL, 0x0010101010106E00UL, 0x0020202020205E00UL, 0x0040404040403E00UL, 0x0080808080807E00UL,
            0x00010101017E0100UL, 0x00020202027C0200UL, 0x00040404047A0400UL, 0x0008080808760800UL, 0x00101010106E1000UL, 0x00202020205E2000UL, 0x00404040403E4000UL, 0x00808080807E8000UL,
            0x000101017E010100UL, 0x000202027C020200UL, 0x000404047A040400UL, 0x0008080876080800UL, 0x001010106E101000UL, 0x002020205E202000UL, 0x004040403E404000UL, 0x008080807E808000UL,
            0x0001017E01010100UL, 0x0002027C02020200UL, 0x0004047A04040400UL, 0x0008087608080800UL, 0x0010106E10101000UL, 0x0020205E20202000UL, 0x0040403E40404000UL, 0x0080807E80808000UL,
            0x00017E0101010100UL, 0x00027C0202020200UL, 0x00047A0404040400UL, 0x0008760808080800UL, 0x00106E1010101000UL, 0x00205E2020202000UL, 0x00403E4040404000UL, 0x00807E8080808000UL,
            0x007E010101010100UL, 0x007C020202020200UL, 0x007A040404040400UL, 0x0076080808080800UL, 0x006E101010101000UL, 0x005E202020202000UL, 0x003E404040404000UL, 0x007E808080808000UL,
            0x7E01010101010100UL, 0x7C02020202020200UL, 0x7A04040404040400UL, 0x7608080808080800UL, 0x6E10101010101000UL, 0x5E20202020202000UL, 0x3E40404040404000UL, 0x7E80808080808000UL
        };

    static MagicBitBoardExtensions()
    {
        for (var i = 0; i < _magicBishopDb.Length; i++)
            _magicBishopDb[i] = new BitBoard[_magicBishopDbLength];

        for (var i = 0; i < _magicBishopDb.Length; ++i)
            _magicRookDb[i] = new BitBoard[_magicRookDbLength];

        var initMagicMovesDb = new[] {
            63,
            0,
            58,
            1,
            59,
            47,
            53,
            2,
            60,
            39,
            48,
            27,
            54,
            33,
            42,
            3,
            61,
            51,
            37,
            40,
            49,
            18,
            28,
            20,
            55,
            30,
            34,
            11,
            43,
            14,
            22,
            4,
            62,
            57,
            46,
            52,
            38,
            26,
            32,
            41,
            50,
            36,
            17,
            19,
            29,
            10,
            13,
            21,
            56,
            45,
            25,
            31,
            35,
            16,
            9,
            12,
            44,
            24,
            15,
            8,
            23,
            7,
            6,
            5
        };

        var squares = new int[64];
        int numSquares;

        for (var i = 0; i < squares.Length; ++i)
        {
            numSquares = 0;
            var temp = _magicMovesBMask[i];
            while (temp != 0)
            {
                var bit = (ulong)((long)temp & -(long)temp);
                squares[numSquares++] = initMagicMovesDb[(int)((bit * 0x07EDD5E59A4E28C2UL) >> 58)];
                temp ^= bit;
            }

            for (temp = 0; temp < One << numSquares; ++temp)
            {
                var tempocc = InitMagicMovesOcc(squares.Slice(0, numSquares), temp);
                _magicBishopDb[i][(tempocc * _magicMovesBMagics[i]) >> 55] = new BitBoard(InitmagicmovesBmoves(i, tempocc));
            }
        }

        squares = new int[64];

        for (var i = 0; i < squares.Length; ++i)
        {
            numSquares = 0;
            var temp = _magicmovesRMask[i];
            while (temp != 0)
            {
                var bit = (ulong)((long)temp & -(long)temp);
                squares[numSquares++] = initMagicMovesDb[(int)((bit * 0x07EDD5E59A4E28C2UL) >> 58)];
                temp ^= bit;
            }

            for (temp = 0; temp < One << numSquares; ++temp)
            {
                var tempocc = InitMagicMovesOcc(squares.Slice(0, numSquares), temp);
                _magicRookDb[i][(tempocc * _magicmovesRMagics[i]) >> 52] = new BitBoard(InitMagicMovesRmoves(i, tempocc));
            }
        }
    }

    static ulong InitMagicMovesOcc(int[] squares, ulong linocc)
    {
        var ret = 0ul;
        for (var i = 0; i < squares.Length; ++i)
        {
            if ((linocc & (One << i)) != 0)
                ret |= One << squares[i];
        }

        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BishopAttacks(this byte square, BitBoard occupied) => _magicBishopDb[square][
            (occupied.And(_magicMovesBMask[square]) * _magicMovesBMagics[square]) >> 55];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard XrayBishopAttacks(this byte square, BitBoard occupied, BitBoard blocker)
    {
        var attacks = BishopAttacks(square, occupied);
        blocker &= attacks;
        return attacks ^ BishopAttacks(square, occupied ^ blocker);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard RookAttacks(this byte square, BitBoard occupied) => _magicRookDb[square][
            (occupied.And(_magicmovesRMask[square]) * _magicmovesRMagics[square]) >> 52];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard XrayRookAttacks(this byte square, BitBoard occupied, BitBoard blocker)
    {
        var attacks = RookAttacks(square, occupied);
        blocker &= attacks;
        return attacks ^ RookAttacks(square, occupied ^ blocker);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard QueenAttacks(this byte square, BitBoard occupied) => BishopAttacks(square, occupied) | RookAttacks(square, occupied);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard XrayQueenAttacks(this byte square, BitBoard occupied, BitBoard blocker)
    {
        var attacks = QueenAttacks(square, occupied);
        blocker &= attacks;
        return attacks ^ QueenAttacks(square, occupied ^ blocker);
    }

    private static ulong InitMagicMovesRmoves(int square, ulong occ)
    {
        var ret = 0ul;
        var rowbits = _ff << (8 * (square / 8));

        var bit = One << square;
        do
        {
            bit <<= 8;
            ret |= bit;
        }
        while (bit != 0 && (bit & occ) == 0);

        bit = One << square;
        do
        {
            bit >>= 8;
            ret |= bit;
        }
        while (bit != 0 && (bit & occ) == 0);

        bit = One << square;
        do
        {
            bit <<= 1;
            if ((bit & rowbits) != 0)
                ret |= bit;
            else
                break;
        }
        while ((bit & occ) == 0);

        bit = One << square;
        do
        {
            bit >>= 1;
            if ((bit & rowbits) != 0)
                ret |= bit;
            else
                break;
        }
        while ((bit & occ) == 0);

        return ret;
    }

    private static ulong InitmagicmovesBmoves(int square, ulong occ)
    {
        var ret = 0UL;
        var rowbits = _ff << (8 * (square / 8));

        var bit = One << square;
        var bit2 = bit;
        do
        {
            bit <<= 8 - 1;
            bit2 >>= 1;
            if ((bit2 & rowbits) != 0)
                ret |= bit;
            else
                break;
        }
        while (bit != 0 && (bit & occ) == 0);

        bit = One << square;
        bit2 = bit;
        do
        {
            bit <<= 8 + 1;
            bit2 <<= 1;
            if ((bit2 & rowbits) != 0)
                ret |= bit;
            else
                break;
        }
        while (bit != 0 && (bit & occ) == 0);

        bit = One << square;
        bit2 = bit;
        do
        {
            bit >>= 8 - 1;
            bit2 <<= 1;
            if ((bit2 & rowbits) != 0)
                ret |= bit;
            else
                break;
        }
        while (bit != 0 && (bit & occ) == 0);

        bit = One << square;
        bit2 = bit;
        do
        {
            bit >>= 8 + 1;
            bit2 >>= 1;
            if ((bit2 & rowbits) != 0)
                ret |= bit;
            else
                break;
        }
        while (bit != 0 && (bit & occ) == 0);

        return ret;
    }
}