using System;

using Ion.Net.Connections;

namespace IHI.Server.Net.Messages
{
    internal class PacketProcessor
    {
        internal User fUser;

        internal PacketProcessor(User User)
        {
            this.fUser = User;

            IonTcpConnection Connection = this.fUser.GetConnection();
            Type ThisType = typeof(PacketProcessor);

            Connection.AddHandler(196, PacketHandlerPriority.DefaultAction, ThisType.GetMethod("Process_Pong"));
            Connection.AddHandler(204, PacketHandlerPriority.DefaultAction, ThisType.GetMethod("Process_SSOTicket"));
            Connection.AddHandler(206, PacketHandlerPriority.DefaultAction, ThisType.GetMethod("Process_InitCrypto"));
        }

        internal void Process_Pong(IncomingMessage Message)
        {
            this.fUser.Pong();
        }

        internal void Process_InitCrypto(IncomingMessage Message)
        {
            this.fUser.GetPacketSender().Send_InitCrypto("166deece3b76b25aa9d5078522c7eb0a", false);
        }

        internal void Process_SSOTicket(IncomingMessage Message)
        {
            Core.GetUserDistributor().GetUser(Message.PopPrefixedString(), this.fUser.GetConnection().ipAddress);
        }
    }
}
