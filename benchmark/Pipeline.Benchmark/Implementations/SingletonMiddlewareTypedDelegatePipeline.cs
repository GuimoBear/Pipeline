using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Implementations
{
    public class SingletonMiddlewareTypedDelegatePipeline<TMessage>
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public SingletonMiddlewareTypedDelegatePipeline(IReadOnlyList<Type> middlewareTypes)
        {
            _middlewareTypes = middlewareTypes.Reverse().ToList();
        }

        public async Task Execute(TMessage message, IServiceProvider services, Func<MessageContext<TMessage>, Task> last)
        {
            var ctx = CreateContext(message, services);
            Func<MessageContext<TMessage>, Task> source = last;
            foreach (var middlewareExecutor in CreateMiddlewareExecutors(services))
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            await source(ctx);
        }

        private Func<MessageContext<TMessage>, Task> CreatePipelineLevelExecutor(Func<MessageContext<TMessage>, Task> source, Func<MessageContext<TMessage>, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static Dictionary<Type, IMiddleware<TMessage>> _middlewareExecutorsCache
            = new Dictionary<Type, IMiddleware<TMessage>>();

        private static MessageContext<TMessage> CreateContext(TMessage message, IServiceProvider services)
            => new MessageContext<TMessage>(message, services);

        private IEnumerable<Func<MessageContext<TMessage>, Func<Task>, Task>> CreateMiddlewareExecutors(IServiceProvider services)
        {
            foreach (var middlewareType in _middlewareTypes)
            {
                if (!_middlewareExecutorsCache.TryGetValue(middlewareType, out var middlewareExecutor))
                {
                    var middleware = services.GetService(middlewareType) as IMiddleware<TMessage>;
                    middlewareExecutor = middleware;
                    _middlewareExecutorsCache.Add(middlewareType, middlewareExecutor);
                }
                yield return middlewareExecutor.Invoke;
            }
        }
    }
}
