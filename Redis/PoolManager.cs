using System;
using System.Buffers;

namespace Framework.Caching.Redis
{
    internal class PoolManager<T> : IDisposable
    {
        private const int MinPoolLength = 512;
        private static readonly ArrayPool<T> m_pool = ArrayPool<T>.Create();

        public PoolManager(int length)
        {
            // TODO calculate based on 2^N power
            Buffer = m_pool.Rent(Math.Max(MinPoolLength, length));
        }

        public T[] Buffer { get; }

        public void Dispose()
        {
            m_pool.Return(Buffer);
        }
    }
}