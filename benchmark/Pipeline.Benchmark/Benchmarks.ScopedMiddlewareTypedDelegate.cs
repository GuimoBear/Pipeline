using BenchmarkDotNet.Attributes;
using Pipeline.Benchmark.Implementations;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    [Description("Scoped typed delegate")]
    public class ScopedDelegatePipelineBenchmark : BenchmarkBase
    {
        private ScopedMiddlewareTypedDelegatePipeline<Message> pipeline;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            pipeline = new ScopedMiddlewareTypedDelegatePipeline<Message>(middlewareTypes);
        }

        [Benchmark(Description = "Scoped typed delegate pipeline executor")]
        public async Task<Message> ExecuteTypedDelegatePipeline()
        {
            using var scope = scopeFactory.CreateScope();
            var message = new Message();
            await pipeline.Execute(message, scope.ServiceProvider);
            return message;
        }
    }
}
