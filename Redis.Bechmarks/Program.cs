using BenchmarkDotNet.Running;

namespace NRedis.Bechmarks
{
    public class Program
    {
        private static void Main() => BenchmarkRunner.Run<Benchmark>();
    }
}