using System;
using System.Collections.Generic;

namespace Pipeline
{
    public abstract class MessageContextBase
    {
        public IServiceProvider Services { get; }

        protected MessageContextBase(IServiceProvider services)
        {
            Services = services;
        }

        public abstract object GetMessageObject();
    }

    public class MessageContext<TMessage> : MessageContextBase
    {
        public TMessage Message { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

        private MessageContext(TMessage message, IServiceProvider services) : base(services)
        {
            Message = message;
        }

        public override object GetMessageObject()
            => Message;

        public static MessageContext<TMessage> Create(object message, IServiceProvider services)
            => new MessageContext<TMessage>((TMessage)message, services);
    }
}
