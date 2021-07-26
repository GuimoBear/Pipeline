using BenchmarkDotNet.Attributes;
using Pipeline.Benchmark.Implementations;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    [Description("Delegate")]
    public class DelegatePipelineBenchmark : BenchmarkBase
    {
        private DelegatePipeline pipeline;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            pipeline = new DelegatePipeline(middlewareTypes);
        }

        [Benchmark(Description = "Delegate pipeline executor")]
        public async Task<Message> ExecuteReflectionPipeline()
        {
            using var scope = scopeFactory.CreateScope();
            var message = new Message();
            await pipeline.Execute(message, scope.ServiceProvider);
            return message;
        }
    }
}
