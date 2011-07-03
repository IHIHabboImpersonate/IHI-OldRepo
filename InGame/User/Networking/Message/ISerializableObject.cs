
namespace IHI.Server.Networking.Messages
{
    public interface ISerializableObject
    {
        void Serialize(OutgoingMessage message);
    }
}
