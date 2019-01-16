using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Framework.Caching.Redis;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Bechmarks
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmark>();
        }
    }
}