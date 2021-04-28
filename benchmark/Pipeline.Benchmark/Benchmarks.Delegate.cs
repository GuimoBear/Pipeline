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
        public async Task<MessageContext<Message>> ExecuteDelegatePipeline()
        {
            using var scope = scopeFactory.CreateScope();
            MessageContext<Message> context = null;
            await pipeline.Execute(typeof(Message), new Message(), scope.ServiceProvider, _ctx =>
            {
                context = _ctx as MessageContext<Message>;
                return Task.CompletedTask;
            });
            return context;
        }
    }
}
