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
            GetExecutor(typeof(MessageContext<>).MakeGenericType(typeof(TMessage)));
        }

        public async Task Execute(TMessage message, IServiceProvider services)
        {
            var ctx = CreateContext(message, services);
            await GetExecutor(ctx.GetType())(ctx);
        }

        private Func<MessageContext<TMessage>, Task> _executor = null;

        private Func<MessageContext<TMessage>, Task> GetExecutor(Type contextType)
        {
            if (_executor is null)
            {
                _executor = _ => Task.CompletedTask;
                foreach (var middlewareExecutor in CreateMiddlewareExecutors())
                    _executor = CreatePipelineLevelExecutor(_executor, middlewareExecutor);
            }
            return _executor;
        }

        private Func<MessageContext<TMessage>, Task> CreatePipelineLevelExecutor(Func<MessageContext<TMessage>, Task> source, Func<MessageContext<TMessage>, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static MessageContext<TMessage> CreateContext(TMessage message, IServiceProvider services)
            => MessageContext<TMessage>.Create(message, services);

        private IEnumerable<Func<MessageContext<TMessage>, Func<Task>, Task>> CreateMiddlewareExecutors()
        {
            foreach (var middlewareType in _middlewareTypes)
                yield return (ctx, next) =>  (ctx.Services.GetService(middlewareType) as IMiddleware<TMessage>).Invoke(ctx, next);
        }
    }
}
