using BenchmarkDotNet.Attributes;
using Pipeline.Benchmark.Implementations;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    [Description("ILGenerator")]
    public class ILGeneratorPipelineBenchmark : BenchmarkBase
    {
        private ILGeneratorPipeline pipeline;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            pipeline = new ILGeneratorPipeline(middlewareTypes, typeof(Message));
        }

        [Benchmark(Description = "ILGenerator pipeline executor")]
        public async Task<Message> ILGeneratorPipelineExecutor()
        {
            using var scope = scopeFactory.CreateScope();
            var message = new Message();
            await pipeline.Execute(message, scope.ServiceProvider);
            return message;
        }
    }
}
