using System;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Middlewares
{
    class SecondMiddleware : IMiddleware<Message>
    {
        public async Task Invoke(MessageContext<Message> message, Func<Task> next)
        {
            message.Message.SecondMiddlewareExecuted = true;
            await next();
        }
    }
}
