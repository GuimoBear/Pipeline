using BenchmarkDotNet.Attributes;
using Pipeline.Benchmark.Implementations;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    [Description("Cached reflection")]
    public class CachedReflectionPipelineBenchmark : BenchmarkBase
    {
        private CachedReflectionPipeline pipeline;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            pipeline = new CachedReflectionPipeline(middlewareTypes, typeof(Message));
        }

        [Benchmark(Description = "Cached reflection pipeline executor")]
        public async Task<Message> ExecuteReflectionPipeline()
        {
            using var scope = scopeFactory.CreateScope();
            var message = new Message();
            await pipeline.Execute(message, scope.ServiceProvider);
            return message;
        }
    }
}
