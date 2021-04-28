using BenchmarkDotNet.Attributes;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    [Description("Expression")]
    public class ExpressionPipelineBenchmark : BenchmarkBase
    {
        private Pipeline pipeline;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            pipeline = new Pipeline(middlewareTypes);
        }

        [Benchmark(Description = "Expression pipeline executor")]
        public async Task<MessageContext<Message>> ExecuteExpressionPipeline()
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
