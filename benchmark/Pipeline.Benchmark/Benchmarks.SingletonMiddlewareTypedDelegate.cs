using BenchmarkDotNet.Attributes;
using Pipeline.Benchmark.Implementations;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pipeline.Benchmark
{
    [Description("Singleton typed delegate")]
    public class TypedDelegatePipelineBenchmark : BenchmarkBase
    {
        private SingletonMiddlewareTypedDelegatePipeline<Message> pipeline;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            pipeline = new SingletonMiddlewareTypedDelegatePipeline<Message>(middlewareTypes);
        }

        [Benchmark(Description = "Singleton typed delegate pipeline executor")]
        public async Task<MessageContext<Message>> ExecuteTypedDelegatePipeline()
        {
            using var scope = scopeFactory.CreateScope();
            MessageContext<Message> context = null;
            await pipeline.Execute(new Message(), scope.ServiceProvider, _ctx => Task.CompletedTask);
            return context;
        }
    }
}
