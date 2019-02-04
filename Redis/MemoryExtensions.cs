using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Framework.Caching.Redis
{
    internal static class MemoryExtensions
    {
        public static byte[] AsBytes(this in Memory<byte> memory)
        {
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> buffer))
                return buffer.Array;

            throw new InvalidOperationException(); // TDOO ecceptions
        }

        public static byte[] AsBytes(this in ReadOnlyMemory<byte> memory)
        {
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> buffer))
                return buffer.Array;

            throw new InvalidOperationException(); // TDOO ecceptions
        }

        public static ReadOnlyMemory<byte> AsMemory(this in ReadOnlySequence<byte> buffer)
        {
            var position = buffer.Start;
            if (buffer.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                return memory;

            throw new InvalidOperationException(); // TODO exception
        }

        public static ReadOnlySpan<byte> AsSpan(this in ReadOnlySequence<byte> buffer)
        {
            return buffer.AsMemory().Span;
        }
    }
}