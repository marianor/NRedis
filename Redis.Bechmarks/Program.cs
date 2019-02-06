using BenchmarkDotNet.Running;

namespace Framework.Caching.Redis.Bechmarks
{
    public class Program
    {
        private static void Main(string[] args) => BenchmarkRunner.Run<Benchmark>();
    }
}