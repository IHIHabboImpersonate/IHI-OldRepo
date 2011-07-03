
using IHI.Server.Net.Messages;

namespace IHI.Server.Messenger
{
    public class Category : ISerializableObject
    {
        private uint fID;
        private string fName;

        public Category(uint ID, string Name)
        {
            this.fID = ID;
            this.fName = Name;
        }
        
        public void Serialize(OutgoingMessage Message)
        {
            Message.AppendUInt32(this.fID);
            Message.AppendString(this.fName);
        }

        public uint GetID()
        {
            return this.fID;
        }

        public string GetName()
        {
            return this.fName;
        }
    }
}
