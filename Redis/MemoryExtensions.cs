using NRedis.Properties;
using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace NRedis
{
    internal static class MemoryExtensions
    {
        public static ArraySegment<T> AsSegment<T>(this in Memory<T> memory)
        {
            try
            {
                ReadOnlyMemory<T> readOnlyMemory = memory;
                return readOnlyMemory.AsSegment();
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException(Resources.CannotMarshalFromTypeToType.Format(typeof(Memory<T>), typeof(ArraySegment<T>)));
            }
        }

        public static ArraySegment<T> AsSegment<T>(this in ReadOnlyMemory<T> memory)
        {
            if (MemoryMarshal.TryGetArray(memory, out var segment))
                return segment;

            throw new InvalidCastException(Resources.CannotMarshalFromTypeToType.Format(typeof(ReadOnlyMemory<T>), typeof(ArraySegment<T>)));
        }

        public static ReadOnlySpan<T> AsSpan<T>(this in ReadOnlySequence<T> buffer)
        {
            // TODO I should consider the case there is more than 1 segment ??
            var position = buffer.Start;
            if (buffer.TryGet(ref position, out var memory))
                return memory.Span;

            throw new InvalidCastException(Resources.CannotMarshalFromTypeToType.Format(typeof(ReadOnlySequence<T>), typeof(ReadOnlySpan<T>)));
        }
    }
}
