using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Networking.Messages
{
    public abstract class OutgoingMessage
    {
        protected IInternalOutgoingMessage fInternalOutgoingMessage = new InternalOutgoingMessage();

        public abstract OutgoingMessage Send(IMessageable Target);
        public OutgoingMessage Send(IEnumerable<IMessageable> Targets, bool SendOncePerConnection = false)
        {
            if (SendOncePerConnection)
                throw new NotImplementedException();
            foreach (IMessageable Target in Targets)
            {
                Target.SendMessage(this.fInternalOutgoingMessage);
            }
            return this;
        }
    }
}