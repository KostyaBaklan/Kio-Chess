﻿using Engine.Book.Interfaces;
using Engine.DataStructures;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Book.Services
{
    public class DataKeyService : IDataKeyService
    {
        private readonly short _depth;
        private readonly LinkedList<short> _keys;

        public DataKeyService(IConfigurationProvider configurationProvider)
        {
            _depth = configurationProvider.BookConfiguration.Depth;
            _keys = new LinkedList<short>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get()
        {
            return string.Join("-", _keys.OrderBy(x=>x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short key)
        {
            _keys.AddLast(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delete()
        {
            _keys.RemoveLast();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _keys.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(IEnumerable<MoveBase> history)
        {
            return string.Join("-", history.Take(_depth).Select(m => m.Key).OrderBy(x => x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(ref MoveKeyList span)
        {
            if(span.Count == 0) return string.Empty;

            span.Sort();

            StringBuilder builder = new StringBuilder();

            byte last = (byte)(span.Count - 1);
            for (byte i = 0; i < last; i++)
            {
                builder.Append($"{span[i]}-");
            }

            builder.Append(span[last]);

            return builder.ToString();
        }
    }
}
