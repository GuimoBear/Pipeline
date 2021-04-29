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
        public async Task<MessageContext<Message>> ExecuteTypedDelegatePipeline()
        {
            using var scope = scopeFactory.CreateScope();
            MessageContext<Message> context = null;
            await pipeline.Execute(typeof(Message), new Message(), scope.ServiceProvider, _ctx =>
            {
                context = _ctx;
                return Task.CompletedTask;
            });
            return context;
        }
    }
}
