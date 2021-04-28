using System;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Middlewares
{
    class ThirdMiddleware : IMiddleware<Message>
    {
        public async Task Invoke(MessageContext<Message> message, Func<Task> next)
        {
            message.Message.ThirdMiddlewareExecuted = true;
            await next();
        }
    }
}
