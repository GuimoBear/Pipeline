using BenchmarkDotNet.Running;
using System;

namespace Pipeline.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iterations: " + Config.Iterations);
            new BenchmarkSwitcher(typeof(BenchmarkBase).Assembly).Run(args, new Config());
        }
    }
}
