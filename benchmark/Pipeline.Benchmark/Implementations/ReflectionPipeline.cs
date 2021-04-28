using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Implementations
{
    internal class ReflectionPipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public ReflectionPipeline(IReadOnlyList<Type> middlewareTypes)
        {
            _middlewareTypes = middlewareTypes.Reverse().ToList();
        }

        public async Task Execute(Type messageType, object message, IServiceProvider services, Func<object, Task> last)
        {
            var ctx = CreateContext(message, services);
            Func<object, Task> source = async context => await last((context as MessageContextBase).GetMessageObject());

            var middlewareExecutors = CreateMiddlewareExecutors(services, messageType);

            foreach (var middlewareExecutor in middlewareExecutors)
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            await source(ctx);
        }

        private Func<object, Task> CreatePipelineLevelExecutor(Func<object, Task> source, Func<object, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static readonly ConcurrentDictionary<Type, MethodInfo> _contextConstructorsCache
            = new ConcurrentDictionary<Type, MethodInfo>();

        private static readonly ConcurrentDictionary<Type, MethodInfo> _middlewareExecutorsCache
            = new ConcurrentDictionary<Type, MethodInfo>();

        private static object CreateContext(object message, IServiceProvider services)
        {
            var messageType = message.GetType();
            if (!_contextConstructorsCache.TryGetValue(messageType, out var constructor))
            {
                var messageContextType = typeof(MessageContext<>).MakeGenericType(messageType);
                var parameters = new Type[] { typeof(object), typeof(IServiceProvider) };
                constructor = messageContextType.GetMethod("Create", parameters);
                _contextConstructorsCache.TryAdd(messageType, constructor);
            }
            return constructor.Invoke(null, new object[] { message, services });
        }

        private IEnumerable<Func<object, Func<Task>, Task>> CreateMiddlewareExecutors(IServiceProvider services, Type messageType)
        {
            var messageContextType = typeof(MessageContext<>).MakeGenericType(messageType);
            foreach (var middlewareType in _middlewareTypes)
            {
                if (!_middlewareExecutorsCache.TryGetValue(middlewareType, out var middlewareExecutor))
                {
                    middlewareExecutor = middlewareType.GetMethod("Invoke", new Type[] { messageContextType, typeof(Func<Task>) });
                    _middlewareExecutorsCache.TryAdd(middlewareType, middlewareExecutor);
                }
                yield return (obj, next) => middlewareExecutor.Invoke(services.GetService(middlewareType), new object[] { obj, next }) as Task;
            }
        }
    }
}
