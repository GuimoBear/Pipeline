using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Implementations
{
    public class ScopedMiddlewareTypedDelegatePipeline<TMessage>
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public ScopedMiddlewareTypedDelegatePipeline(IReadOnlyList<Type> middlewareTypes)
        {
            _middlewareTypes = middlewareTypes.Reverse().ToList();
        }

        public async Task Execute(Type messageType, TMessage message, IServiceProvider services, Func<MessageContext<TMessage>, Task> last)
        {
            var ctx = CreateContext(message, services);
            Func<MessageContext<TMessage>, Task> source = last;

            var middlewareExecutors = CreateMiddlewareExecutors(services, messageType);

            foreach (var middlewareExecutor in middlewareExecutors)
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            await source(ctx);
        }

        private Func<MessageContext<TMessage>, Task> CreatePipelineLevelExecutor(Func<MessageContext<TMessage>, Task> source, Func<MessageContext<TMessage>, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static MessageContext<TMessage> CreateContext(TMessage message, IServiceProvider services)
            => new MessageContext<TMessage>(message, services);

        private IEnumerable<Func<MessageContext<TMessage>, Func<Task>, Task>> CreateMiddlewareExecutors(IServiceProvider services, Type messageType)
        {
            foreach (var middlewareType in _middlewareTypes)
                yield return (services.GetService(middlewareType) as IMiddleware<TMessage>).Invoke;
        }
    }
}
