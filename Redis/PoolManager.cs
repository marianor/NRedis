using System;
using System.Buffers;

namespace Framework.Caching.Redis
{
    internal class PoolManager<T> : IDisposable
    {
        private const int MinimunLength = 512;
        private static readonly ArrayPool<T> m_pool = ArrayPool<T>.Shared;

        public PoolManager(int length)
        {
            var minimunLength = MinimunLength;
            if (length > minimunLength)
                minimunLength = GetNextPower(length);

            Buffer = m_pool.Rent(minimunLength);
        }

        public T[] Buffer { get; }

        public void Dispose()
        {
            m_pool.Return(Buffer);
        }

        private static int GetNextPower(int value)
        {
            value = value > 0 ? value - 1 : 0;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return ++value;
        }
    }
}