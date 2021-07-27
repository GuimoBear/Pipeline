using Sigil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Implementations
{
    internal class ILGeneratorPipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;
        private readonly Type _messageType;

        private readonly Func<MessageContextBase, Task> _executor = null;

        public ILGeneratorPipeline(IReadOnlyList<Type> middlewareTypes, Type messageType)
        {
            _middlewareTypes = middlewareTypes.Reverse().ToList();
            _messageType = messageType;
            _executor = GetExecutor(typeof(MessageContext<>).MakeGenericType(_messageType));
        }

        public async Task Execute(object message, IServiceProvider services)
        {
            var ctx = CreateContext(message.GetType(), message, services);
            await _executor(ctx);
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

        private static readonly IDictionary<Type, Func<object, IServiceProvider, MessageContextBase>> _constructorDelegates
            = new ConcurrentDictionary<Type, Func<object, IServiceProvider, MessageContextBase>>();

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

                _constructorDelegates.Add(messageType, ctor);
            }
            return ctor(message, services);
        }

        private static readonly ConcurrentDictionary<Type, Func<object, MessageContextBase, Func<Task>, Task>> _middlewareExecutorsCache
            = new();

        private IEnumerable<Func<MessageContextBase, Func<Task>, Task>> CreateMiddlewareExecutors(Type messageContextType)
        {
            foreach (var middlewareType in _middlewareTypes)
            {
                if (!_middlewareExecutorsCache.TryGetValue(middlewareType, out var middlewareInvokeDelegate))
                {
                    var middlewareExecutor = middlewareType.GetMethod("Invoke", new Type[] { messageContextType, typeof(Func<Task>) });

                    var emmiter = Emit<Func<object, MessageContextBase, Func<Task>, Task>>
                        .NewDynamicMethod($"{middlewareType.Name}_Invoke")
                        .LoadArgument(0)
                        .CastClass(middlewareType)
                        .LoadArgument(1)
                        .CastClass(messageContextType)
                        .LoadArgument(2)
                        .Call(middlewareExecutor)
                        .Return();

                    middlewareInvokeDelegate = emmiter.CreateDelegate();

                    _middlewareExecutorsCache.TryAdd(middlewareType, middlewareInvokeDelegate);
                }
                yield return async (obj, next) => await middlewareInvokeDelegate(obj.Services.GetService(middlewareType), obj, next);
            }
        }
    }
}
