
using IHI.Server.Networking.Messages;

namespace IHI.Server.Messenger
{
    public class Category : ISerializableObject
    {
        private int fID;
        private string fName;

        public Category(int ID, string Name)
        {
            this.fID = ID;
            this.fName = Name;
        }
        
        public void Serialize(OutgoingMessage Message)
        {
            Message.AppendInt32(this.fID);
            Message.AppendString(this.fName);
        }

        public int GetID()
        {
            return this.fID;
        }

        public string GetName()
        {
            return this.fName;
        }
    }
}
