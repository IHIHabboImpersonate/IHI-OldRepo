using IHI.Server.Messenger;
using IHI.Server.SubPackets;
using IHI.Server.Habbos;

namespace IHI.Server.Networking.Messages
{
    public class PacketSender
    {
        /// <summary>
        /// The user object this PacketSender is assigned to.
        /// </summary>
        internal Habbo fUser;

        internal PacketSender(Habbo User)
        {
            this.fUser = User;
        }

        public Habbo GetUser()
        {
            return this.fUser;
        }
    }
}
