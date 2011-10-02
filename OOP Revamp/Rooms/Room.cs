using IHI.Server.Networking.Messages;

namespace IHI.Server.Rooms
{
    public abstract class Room : IMessageable
    {
        #region IMessageable Members

        public IMessageable SendMessage(IInternalOutgoingMessage Message)
        {
            return this;
        }

        #endregion
    }
}