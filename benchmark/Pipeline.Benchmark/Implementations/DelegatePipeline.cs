using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Implementations
{
    internal class DelegatePipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public DelegatePipeline(IReadOnlyList<Type> middlewareTypes)
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

        private static readonly ConcurrentDictionary<Type, Func<object, IServiceProvider, object>> _contextConstructorsCache
            = new ConcurrentDictionary<Type, Func<object, IServiceProvider, object>>();

        private static readonly ConcurrentDictionary<Type, Func<object, Func<Task>, Task>> _middlewareExecutorsCache
            = new ConcurrentDictionary<Type, Func<object, Func<Task>, Task>>();

        private static object CreateContext(object message, IServiceProvider services)
        {
            var messageType = message.GetType();
            if (!_contextConstructorsCache.TryGetValue(messageType, out var constructor))
            {
                var messageContextType = typeof(MessageContext<>).MakeGenericType(messageType);
                var parameters = new Type[] { typeof(object), typeof(IServiceProvider) };
                constructor = messageContextType.GetMethod("Create", parameters).CreateDelegate<Func<object, IServiceProvider, object>>();
                _contextConstructorsCache.TryAdd(messageType, constructor);
            }
            return constructor(message, services);
        }

        private IEnumerable<Func<object, Func<Task>, Task>> CreateMiddlewareExecutors(IServiceProvider services, Type messageType)
        {
            var messageContextType = typeof(MessageContext<>).MakeGenericType(messageType);
            foreach (var middlewareType in _middlewareTypes)
            {
                if (!_middlewareExecutorsCache.TryGetValue(middlewareType, out var middlewareExecutor))
                {
                    var middlewareInstance = services.GetService(middlewareType);
                    var method = middlewareType.GetMethod("Invoke", new Type[] { messageContextType, typeof(Func<Task>) });
                    var @delegate = method.CreateDelegate(typeof(Func<,,>).MakeGenericType(messageContextType, typeof(Func<Task>), typeof(Task)), middlewareInstance);
                    
                    middlewareExecutor = (obj, next) => @delegate.DynamicInvoke(obj, next) as Task;
                    _middlewareExecutorsCache.TryAdd(middlewareType, middlewareExecutor);
                }
                yield return (obj, next) =>  middlewareExecutor(obj, next);
            }
        }
    }
}
