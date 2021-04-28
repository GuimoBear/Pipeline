using BenchmarkDotNet.Running;
using System;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iterations: " + Config.Iterations);

            var delegateBenchmark = new DelegatePipelineBenchmark();
            delegateBenchmark.Setup();
            _ = delegateBenchmark.ExecuteDelegatePipeline().ConfigureAwait(true).GetAwaiter().GetResult();

            var reflectionBenchmark = new ReflectionPipelineBenchmark();
            reflectionBenchmark.Setup();
            _ = reflectionBenchmark.ExecuteReflectionPipeline().ConfigureAwait(true).GetAwaiter().GetResult();

            new BenchmarkSwitcher(typeof(BenchmarkBase).Assembly).Run(args, new Config());
        }
    }
}
