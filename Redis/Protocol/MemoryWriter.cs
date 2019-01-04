﻿using System;
using System.Globalization;
using System.Text;

namespace Framework.Caching.Protocol
{
    internal class MemoryWriter
    {
        private readonly Memory<byte> _memory;

        public MemoryWriter(Memory<byte> memory)
        {
            _memory = memory;
        }

        public int Position { get; private set; }

        public void Write(byte[] buffer)
        {
            var span = _memory.Slice(Position).Span;
            for (var i = 0; i < buffer.Length; i++)
                span[i] = buffer[i];

            Position += buffer.Length;
        }

        public void Write(string value)
        {
            Write(Encoding.UTF8.GetBytes(value));
        }

        public void Write(long value)
        {
            Write(value.ToString(CultureInfo.CurrentCulture));
        }

        public void Write(double value)
        {
            Write(value.ToString(CultureInfo.CurrentCulture));
        }
    }
}