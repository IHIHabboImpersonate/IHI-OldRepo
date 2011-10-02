using IHI.Server.Habbos;

namespace IHI.Server.Networking.Messages
{
    public delegate void PacketHandler(Habbo sender, IncomingMessage message);
}