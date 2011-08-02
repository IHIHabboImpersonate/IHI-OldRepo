using IHI.Server.Networking;
using IHI.Server.Habbos;

namespace IHI.Server.Networking.Messages
{
    internal class PacketProcessor
    {
        /// <summary>
        /// The user object this PacketProcessor is assigned to.
        /// </summary>
        internal Habbo fUser;

        internal PacketProcessor(Habbo User)
        {
            this.fUser = User;
            
            IonTcpConnection Connection = this.fUser.GetConnection();

            Connection.AddHandler(196, PacketHandlerPriority.DefaultAction, new PacketHandler(Process_Pong));
            Connection.AddHandler(512, PacketHandlerPriority.DefaultAction, new PacketHandler(Process_Disconnect));
        }

        internal void Process_Pong(Habbo Sender, IncomingMessage Message)
        {
            Sender.Pong();
        }
        internal void Process_Disconnect(Habbo Sender, IncomingMessage Message)
        {
            Sender.GetConnection().Disconnect();
        }
    }
}
