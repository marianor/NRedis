using System;
using System.Buffers.Text;

namespace Framework.Caching.Redis.Protocol
{
    internal class MemoryWriter
    {
        private readonly Memory<byte> _memory;

        public MemoryWriter(Memory<byte> memory)
        {
            _memory = memory;
        }

        public int Position { get; private set; }

        public void WriteRaw(byte b)
        {
            _memory.Span[Position++] = b;
        }

        public void WriteRaw(byte[] buffer)
        {
            var span = _memory.Slice(Position).Span;
            for (var i = 0; i < buffer.Length; i++)
                span[i] = buffer[i];

            Position += buffer.Length;
        }

        public void Write(string value)
        {
            WriteRaw(Resp.Encoding.GetBytes(value));
        }

        public void Write(long value)
        {
            var span = _memory.Slice(Position).Span;
            Utf8Formatter.TryFormat(value, span, out int bytesWritten);
            Position += bytesWritten;
        }

        public void Write(double value)
        {
            var span = _memory.Slice(Position).Span;
            Utf8Formatter.TryFormat(value, span, out int bytesWritten);
            Position += bytesWritten;
        }
    }
}