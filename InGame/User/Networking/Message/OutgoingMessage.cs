using System;
using System.Collections.Generic;

namespace IHI.Server.Networking.Messages
{
    public abstract class OutgoingMessage
    {
        protected readonly IInternalOutgoingMessage InternalOutgoingMessage = new InternalOutgoingMessage();

        public abstract OutgoingMessage Send(IMessageable target);

        public OutgoingMessage Send(IEnumerable<IMessageable> targets, bool sendOncePerConnection = false)
        {
            if (sendOncePerConnection)
                throw new NotImplementedException();
            foreach (var target in targets)
            {
                Send(target);
            }
            return this;
        }
    }
}