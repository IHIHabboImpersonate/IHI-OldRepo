using IHI.Server.Networking.Messages;

namespace IHI.Server.Rooms
{
    public abstract class Room : IMessageable
    {
        #region IMessageable Members

        public IMessageable SendMessage(IInternalOutgoingMessage message)
        {
            return this;
        }

        #endregion
    }
}