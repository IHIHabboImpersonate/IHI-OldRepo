using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IHI.Server.Networking.Messages;

namespace IHI.Server.Rooms
{
    public abstract class Room : IMessageable
    {
        public IMessageable SendMessage(IInternalOutgoingMessage Message)
        {

            return this;
        }
    }
}
