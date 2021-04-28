using System;
using System.Threading.Tasks;

namespace Pipeline.Benchmark.Middlewares
{
    public class FirstMiddleware : IMiddleware<Message>
    {
        public async Task Invoke(MessageContext<Message> message, Func<Task> next)
        {
            message.Message.FirstMiddlewareExecuted = true;
            await next();
        }
    }
}
