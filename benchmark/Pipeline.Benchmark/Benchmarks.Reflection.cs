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
            pipeline = new ReflectionPipeline(middlewareTypes, typeof(Message));
        }

        [Benchmark(Description = "Reflection pipeline executor")]
        public async Task<Message> ExecuteReflectionPipeline()
        {
            using var scope = scopeFactory.CreateScope();
            var message = new Message();
            await pipeline.Execute(message, scope.ServiceProvider);
            return message;
        }
    }
}
