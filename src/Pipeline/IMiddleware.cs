using System;
using System.Threading.Tasks;

namespace Pipeline
{
    public interface IMiddleware<TType>
    {
        Task Invoke(MessageContext<TType> message, Func<Task> next);
    }
}
