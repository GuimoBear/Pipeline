using Microsoft.Extensions.DependencyInjection;
using Pipeline.Benchmark.Middlewares;
using System;
using System.Collections.Generic;

namespace Pipeline.Benchmark
{
    public abstract class BenchmarkBase
    {
        protected static readonly List<Type> middlewareTypes = new List<Type> { typeof(FirstMiddleware), typeof(SecondMiddleware), typeof(ThirdMiddleware) };

        protected IServiceScopeFactory scopeFactory;

        protected void BaseSetup()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<FirstMiddleware>();
            serviceCollection.AddSingleton<SecondMiddleware>();
            serviceCollection.AddSingleton<ThirdMiddleware>();

            scopeFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        }
    }
}
