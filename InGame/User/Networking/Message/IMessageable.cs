namespace IHI.Server.Networking.Messages
{
    public interface IMessageable
    {
        IMessageable SendMessage(IInternalOutgoingMessage message);
    }
}