using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Networking.Messages
{
    public interface IMessageable
    {
        IMessageable SendMessage(IInternalOutgoingMessage Message);
    }
}
