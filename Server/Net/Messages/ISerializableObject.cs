
namespace IHI.Server.Net.Messages
{
    public interface ISerializableObject
    {
        void Serialize(OutgoingMessage message);
    }
}
