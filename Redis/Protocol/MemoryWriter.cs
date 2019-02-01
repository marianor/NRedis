using System;
using System.Buffers.Text;
using System.Globalization;
using System.IO.Pipelines;
using System.Linq;

namespace Framework.Caching.Redis.Protocol
{
    internal class MemoryWriter
    {
        private readonly Memory<byte> _memory;
        //private readonly PipeWriter x;


        public MemoryWriter(Memory<byte> memory)
        {
            //var pipe = new Pipe();
            //x = pipe.Writer;
            _memory = memory;
        }

        public int Position { get; private set; }

        public void Write(byte b)
        {
            _memory.Span[Position++] = b;
        }

        public void Write(byte[] buffer)
        {
            var span = _memory.Slice(Position).Span;
            for (var i = 0; i < buffer.Length; i++)
                span[i] = buffer[i];

            Position += buffer.Length;
        }

        public void Write(string value)
        {
            Write(Resp.Encoding.GetBytes(value));
        }

        public void Write(long value)
        {
            var span = _memory.Slice(Position).Span;
            Utf8Formatter.TryFormat(value, span, out int bytesWritten);
            Position += bytesWritten;
        }
    }
}