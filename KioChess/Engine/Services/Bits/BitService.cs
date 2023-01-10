﻿using Engine.Models.Boards;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Services.Bits
{

    public class BitService : BitServiceBase
    {
        private const ulong _magic = 0x07EDD5E59A4E28C2;

        private readonly byte[] _magicTable;

        public BitService()
        {
            _magicTable = new byte[64];

            ulong bit = 1;
            byte i = 0;
            do
            {
                _magicTable[bit * _magic >> 58] = i;
                i++;
                bit <<= 1;
            } while (bit != 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte BitScanForward(BitBoard b)
        {
            return _magicTable[b.Lsb() * _magic >> 58];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Count(BitBoard b)
        {
            int count = 0;
            while (b.Any())
            {
                b = b.Remove(BitScanForward(b));
                count++;
            }

            return count;
        }
    }
}
