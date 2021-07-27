using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Implementations
{
    internal class ReflectionPipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public ReflectionPipeline(IReadOnlyList<Type> middlewareTypes, Type messageType)
        {
            _middlewareTypes = middlewareTypes.Reverse().ToList();
            GetExecutor(typeof(MessageContext<>).MakeGenericType(messageType));
        }

        public async Task Execute(object message, IServiceProvider services)
        {
            var ctx = CreateContext(message, services);
            await GetExecutor(ctx.GetType())(ctx);
        }

        private Func<MessageContextBase, Task> GetExecutor(Type contextType)
        {
            Func<MessageContextBase, Task> source = _ => Task.CompletedTask;
            foreach (var middlewareExecutor in CreateMiddlewareExecutors(contextType))
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            return source;
        }

        private Func<MessageContextBase, Task> CreatePipelineLevelExecutor(Func<MessageContextBase, Task> source, Func<MessageContextBase, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static MessageContextBase CreateContext(object message, IServiceProvider services)
        {
            var messageType = message.GetType();
            var messageContextType = typeof(MessageContext<>).MakeGenericType(messageType);
            var parameters = new Type[] { messageType, typeof(IServiceProvider) };
            var constructor = messageContextType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
            return constructor.Invoke(new object[] { message, services }) as MessageContextBase;
        }

        private IEnumerable<Func<MessageContextBase, Func<Task>, Task>> CreateMiddlewareExecutors(Type messageContextType)
        {
            foreach (var middlewareType in _middlewareTypes)
            {
                var middlewareExecutor = middlewareType.GetMethod("Invoke", new Type[] { messageContextType, typeof(Func<Task>) });
                yield return async (obj, next) => await (middlewareExecutor.Invoke(obj.Services.GetService(middlewareType), new object[] { obj, next }) as Task);
            }
        }
    }
}
