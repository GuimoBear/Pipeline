using System;
using System.Collections.Generic;

namespace Pipeline
{
    public abstract class MessageContextBase
    {
        public abstract object GetMessageObject();
    }

    public class MessageContext<TMessage> : MessageContextBase
    {
        public TMessage Message { get; }
        public IServiceProvider Services { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

        public MessageContext(TMessage message, IServiceProvider services)
        {
            Message = message;
            Services = services;
        }

        public override object GetMessageObject()
            => Message;

        public static MessageContext<TMessage> Create(object message, IServiceProvider services)
            => new MessageContext<TMessage>((TMessage)message, services);
    }
}
