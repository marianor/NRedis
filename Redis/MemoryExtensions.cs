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

        public static ArraySegment<byte> AsSegment(this in ReadOnlySequence<byte> buffer)
        {
            if (MemoryMarshal.TryGetArray(buffer.AsMemory(), out ArraySegment<byte> segment))
                return segment;

            throw new InvalidOperationException(); // TDOO ecceptions
        }


        public static ReadOnlySpan<byte> AsSpan(this in ReadOnlySequence<byte> buffer)
        {
            return buffer.AsMemory().Span;
        }

        private static ReadOnlyMemory<T> AsMemory<T>(this in ReadOnlySequence<T> buffer)
        {
            var position = buffer.Start;
            if (buffer.TryGet(ref position, out ReadOnlyMemory<T> memory))
                return memory;

            throw new InvalidOperationException(); // TODO exception
        }
    }
}