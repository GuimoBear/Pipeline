using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipeline
{
    public class Pipeline
    {
        private readonly IReadOnlyList<Type> _middlewareTypes;

        public Pipeline(IReadOnlyList<Type> middlewareTypes)
        {
            _middlewareTypes = middlewareTypes.Reverse().ToList();
        }

        private IEnumerable<Func<object, Func<Task>, Task>> CreateMiddlewareExecutors(IServiceProvider services, Type messageType)
        {
            foreach (var middlewareType in _middlewareTypes)
            {
                var middleware = services.GetService(middlewareType);
                var executor = PipelineExpressionFactory.GetMiddlewareExecutor(middleware, messageType);
                yield return async (context, next) => await executor(context, next);
            }
        }

        public async Task Execute(Type messageType, object message, IServiceProvider services, Func<object, Task> last)
        {
            var ctx = PipelineExpressionFactory.GetMessagecontextConstructor(messageType)(message, services);
            Func<object, Task> source = last;

            var middlewareExecutors = CreateMiddlewareExecutors(services, messageType);

            foreach (var middlewareExecutor in middlewareExecutors)
                source = CreatePipelineLevelExecutor(source, middlewareExecutor);
            await source(ctx);
        }

        private Func<object, Task> CreatePipelineLevelExecutor(Func<object, Task> source, Func<object, Func<Task>, Task> current)
        {
            return async context => await current(context, async () => await source(context));
        }
    }
}
