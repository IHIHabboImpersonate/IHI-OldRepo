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
    }
}
