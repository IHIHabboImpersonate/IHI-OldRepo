
using IHI.Server.Messenger;
using IHI.Server.Networking.Messages;

namespace IHI.Server.SubPackets
{
    public class FriendUpdate : ISerializableObject
    {
        private readonly byte[] fData;

        public FriendUpdate(sbyte Action, Friend Friend)
        {
            OutgoingMessage Message = new OutgoingMessage();

            Message.AppendInt32(Action);
            Message.AppendInt32(Friend.GetHabbo().GetID());
            Message.AppendString(Friend.GetHabbo().GetUsername());
            Message.AppendBoolean(true);                            // TODO: Find out
            Message.AppendBoolean(Friend.GetHabbo().IsLoggedIn());
            Message.AppendBoolean(Friend.IsStalkable());

            if (Friend.GetHabbo().IsLoggedIn())
            {
                Message.AppendString(Friend.GetHabbo().GetFigure());
                Message.AppendInt32(Friend.GetLocalCategory());
                Message.AppendString(Friend.GetHabbo().GetMotto());
                Message.AppendString("");
            }
            else
            {
                Message.AppendString("");                                           // Figure not sent because they are offline.
                Message.AppendInt32(Friend.GetLocalCategory());
                Message.AppendString(Friend.GetHabbo().GetMotto());
                Message.AppendString(Friend.GetHabbo().GetLastAccess().ToString());
            }
            
            this.fData = Message.GetBytes();
        }

        public void Serialize(OutgoingMessage Message)
        {
            Message.Append(this.fData);
        }
    }
}