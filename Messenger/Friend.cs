
using IHI.Server.Net.Messages;

namespace IHI.Server.Messenger
{
    public class Friend : ISerializableObject
    {
        private User fUser;             // There user     (Your friend's user)
        private uint fLocalCategory;    // Your category  (What you see them in)
        private uint fRemoteCategory;   // Their category (What they see you in)

        private bool fWasLoggedIn;      // Was the user logged in last Update()? 
        private bool fWasInRoom;        // Was the user in a room last Update()?

        /// <summary>
        /// Construct a new Friend object.
        /// </summary>
        /// <param name="User">The User who is a Friend.</param>
        /// <param name="LocalCategory">The category ID that this Friend appears in.</param>
        /// <param name="RemoteCategory">The category ID that this Friend appears in for the friend.</param>
        public Friend(User User, uint LocalCategory, uint RemoteCategory)
        {
            this.fUser = User;
            this.fLocalCategory = LocalCategory;
            this.fRemoteCategory = RemoteCategory;

            this.fWasLoggedIn = false;
            this.fWasInRoom = false;

            Update(); // Ensure the Friend object is upto date before use.
        }

        /// <summary>
        /// Returns the user object of this friend.
        /// </summary>
        public User GetUser()
        {
            return this.fUser;
        }

        /// <summary>
        /// Returns the category ID which this friend appears is.
        /// </summary>
        public uint GetLocalCategory()
        {
            return this.fLocalCategory;
        }

        /// <summary>
        /// Returns the category ID which you appear in to this friend.
        /// </summary>
        public uint GetRemoteCategory()
        {
            return this.fRemoteCategory;
        }

        /// <summary>
        /// Returns true if the user can be stalked, false otherwise.
        /// A user can only be stalked if they are in a room and have not blocked stalking.
        /// </summary>
        public bool IsStalkable()
        {
            // TODO: Block Flags
            return this.fWasInRoom;
        }

        /// <summary>
        /// Checks for an update by this friend.
        /// </summary>
        /// <returns>Returns true if there was an update, false otherwise.</returns>
        public bool Update()
        {
            bool IsLoggedIn = this.fUser.IsLoggedIn();
            bool IsInRoom = (this.fUser.GetRoom() != null);

            if (this.fWasLoggedIn != IsLoggedIn || this.fWasInRoom != IsInRoom) // Is there an update?
            {
                // Yes, change "was" value to reflect the changes.
                this.fWasLoggedIn = IsLoggedIn; 
                this.fWasInRoom = IsInRoom;

                return true;
            }

            return false;
        }

        public void Serialize(OutgoingMessage Message)
        {
            if (Message.ID != 12)
                Message.AppendBoolean(false);                       // TODO: Find out what this does

            Message.AppendUInt32(this.fUser.GetID());               // User ID
            Message.AppendString(this.fUser.GetUsername());         // Username
            
            Message.AppendBoolean(false);                           // Not sure what this does
            Message.AppendBoolean(this.fUser.IsLoggedIn());         // Logged In
            Message.AppendBoolean(this.fUser.GetRoom() != null);    // In Room
            Message.AppendString(this.fUser.GetFigure());           // Figure
            Message.AppendUInt32(this.fLocalCategory);              // Category ID

            if (this.fUser.IsLoggedIn())
            {
                Message.AppendString(this.fUser.GetMotto());        // Motto
                Message.AppendString("");                           // Last Access (N/A)
            }
            else
            {
                Message.AppendString("Offline");                                // Motto ("Offline" in this case)
                Message.AppendString(this.fUser.GetLastAccess().ToString());    // Last Access
            }
        }

        internal byte[] GetRawUpdate()
        {
            OutgoingMessage Message = new OutgoingMessage();
            Message.AppendBoolean(true);                            // TODO: Find out what this does

            Message.AppendUInt32(this.fUser.GetID());               // User ID
            Message.AppendString(this.fUser.GetUsername());         // Username

            Message.AppendBoolean(true);                            // Not sure what this does
            Message.AppendBoolean(this.fUser.IsLoggedIn());         // Logged In
            Message.AppendBoolean(this.fUser.GetRoom() != null);    // In Room
            Message.AppendString(this.fUser.GetFigure());           // Figure
            Message.AppendUInt32(this.fLocalCategory);              // Category ID

            if (this.fUser.IsLoggedIn())
            {
                Message.AppendString(this.fUser.GetMotto());        // Motto
                Message.AppendString("");                           // Last Access (N/A)
            }
            else
            {
                Message.AppendString("Offline");                                // Motto ("Offline" in this case)
                Message.AppendString(this.fUser.GetLastAccess().ToString());    // Last Access
            }

            return Message.GetBytes();
        }
    }
}
