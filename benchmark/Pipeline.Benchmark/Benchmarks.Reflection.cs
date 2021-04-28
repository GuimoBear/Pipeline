using BenchmarkDotNet.Attributes;
using Pipeline.Benchmark.Implementations;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    [Description("Reflection")]
    public class ReflectionPipelineBenchmark : BenchmarkBase
    {
        private ReflectionPipeline pipeline;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            pipeline = new ReflectionPipeline(middlewareTypes);
        }

        [Benchmark(Description = "Reflection pipeline executor")]
        public async Task<MessageContext<Message>> ExecuteReflectionPipeline()
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
