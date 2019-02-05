using BenchmarkDotNet.Running;

namespace Framework.Caching.Redis.Bechmarks
{
    public class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}