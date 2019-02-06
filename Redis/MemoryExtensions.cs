using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Framework.Caching.Redis
{
    internal static class MemoryExtensions
    {
        public static ArraySegment<T> AsSegment<T>(this in Memory<T> memory)
        {
            ReadOnlyMemory<T> readOnlyMemory = memory;
            return readOnlyMemory.AsSegment();
        }

        public static ArraySegment<T> AsSegment<T>(this in ReadOnlyMemory<T> memory)
        {
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<T> segment))
                return segment;

            throw new InvalidOperationException(); // TDOO ecceptions
        }

        public static ReadOnlySpan<T> AsSpan<T>(this in ReadOnlySequence<T> buffer)
        {
            // TODO I should consider the case there is more than 1 segment
            var position = buffer.Start;
            if (buffer.TryGet(ref position, out ReadOnlyMemory<T> memory))
                return memory.Span;

            throw new InvalidOperationException(); // TODO exception
        }
    }
}