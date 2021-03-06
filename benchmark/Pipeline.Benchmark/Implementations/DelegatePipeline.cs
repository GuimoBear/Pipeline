using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Sigil;

namespace Pipeline.Benchmark.Implementations
{
    internal class DelegatePipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public DelegatePipeline(IReadOnlyList<Type> middlewareTypes)
        {
            _middlewareTypes = middlewareTypes.Reverse().ToList();
        }

        public async Task Execute(object message, IServiceProvider services)
        {
            var ctx = CreateContext(message.GetType(), message, services);
            await GetExecutor(ctx.GetType(), services)(ctx);
        }

        private Func<MessageContextBase, Task> _executor = null;

        private Func<MessageContextBase, Task> GetExecutor(Type contextType, IServiceProvider services)
        {
            if (_executor is null)
            {
                _executor = _ => Task.CompletedTask;
                foreach (var middlewareExecutor in CreateMiddlewareExecutors(services, contextType))
                    _executor = CreatePipelineLevelExecutor(_executor, middlewareExecutor);
            }
            return _executor;
        }

        private Func<MessageContextBase, Task> CreatePipelineLevelExecutor(Func<MessageContextBase, Task> source, Func<MessageContextBase, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }

        private static readonly ConcurrentDictionary<Type, Func<object, Func<Task>, Task>> _middlewareExecutorsCache
            = new ConcurrentDictionary<Type, Func<object, Func<Task>, Task>>();

        private static readonly ConcurrentDictionary<Type, Func<object, IServiceProvider, MessageContextBase>> _constructorDelegates
            = new ();

        private static MessageContextBase CreateContext(Type messageType, object message, IServiceProvider services)
        {
            if (!_constructorDelegates.TryGetValue(messageType, out var ctor))
            {
                var constructorInfo = typeof(MessageContext<>).MakeGenericType(messageType)
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic,
                        Type.DefaultBinder,
                        new Type[] { messageType, typeof(IServiceProvider) },
                        Array.Empty<ParameterModifier>());

                ctor = Emit<Func<object, IServiceProvider, MessageContextBase>>.NewDynamicMethod($"{messageType.Name}_MessageContext_Ctor")
                    .LoadArgument(0)
                    .CastClass(messageType)
                    .LoadArgument(1)
                    .NewObject(constructorInfo)
                    .Return()
                    .CreateDelegate();

                _constructorDelegates.TryAdd(messageType, ctor);
            }
            return ctor(message, services);
        }

        private IEnumerable<Func<MessageContextBase, Func<Task>, Task>> CreateMiddlewareExecutors(IServiceProvider services, Type messageContextType)
        {
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
                yield return (obj, next) => middlewareExecutor(obj, next);
            }
        }
    }
}
