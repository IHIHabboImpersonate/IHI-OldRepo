using System.Collections.Generic;
using System.Data;

using IHI.Server.SubPackets;
using Ion.Storage;

namespace IHI.Server.Messenger
{
    /// <summary>
    /// Handles everything to do with the Messenger/Friends List/Console.
    /// </summary>
    public class Messenger
    {
        #region Fields
        private User fUser;
        private Dictionary<uint, Friend> fFriends;          // Holds all friends of this user. (The key is the Friend's UserId)
        private List<Category> fCategories;                 // Holds all the categories for this user. (The key is ID, the value is the name)
        private byte fBlockFlags;                           // TODO: Add BlockFlags

        private List<FriendUpdate> fUpdateList;             // The updates to send next time SendUpdates() is called.
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a new Messenger.
        /// </summary>
        /// <param name="User">The user who's messenger to manage.</param>
        /// <param name="BlockFlags">Not done yet</param>
        internal Messenger(User User, byte BlockFlags)
        {
            this.fUser = User;
            this.fBlockFlags = BlockFlags;                  // TODO: Add BlockFlags

            this.fUpdateList = new List<FriendUpdate>();    // Prepare to store update data.
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new friend to the friend list.
        /// If they are already a friend, nothing happens.
        /// </summary>
        /// <param name="Friend">The friend to add to the friend list.</param>
        public Messenger AddFriend(Friend Friend)
        {
            lock (this)
            {
                if (this.fFriends.ContainsKey(Friend.GetUser().GetID()))    // Check if they are already a friend.
                    return this;                                                     // They are, abort!
                this.fFriends.Add(Friend.GetUser().GetID(), Friend);        // Nope, not a friend. Add them.
            }

            AddUpdate(new FriendUpdate(FriendUpdateActions.Add, Friend));                               // Add the friend to the update data.

            return this;
        }

        /// <summary>
        /// Remove a Friend from the friend list.
        /// </summary>
        /// <param name="Friend">The Friend to remove.</param>
        public Messenger RemoveFriend(Friend Friend)
        {
            AddUpdate(new FriendUpdate(FriendUpdateActions.Remove, Friend));

            lock (this)
            {
                if (!this.fFriends.ContainsKey(Friend.GetUser().GetID()))   // Does the Friend exist in the friend list?
                    this.fFriends.Remove(Friend.GetUser().GetID());         // Yes, remove it.
            }

            SendUpdates();  // Force an early update.

            return this;
        }

        /// <summary>
        /// Load all friends and categories from the database.
        /// </summary>
        internal void LoadFriendList()
        {
            DataTable FriendsSent; // Friends where you sent the friend request
            DataTable FriendsRecv; // Friends where they sent the friend request
            DataTable Categories;  // Categories to put the friends in.

            #region Queries
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("myuserid", this.fUser.GetID());
                FriendsSent = dbClient.ReadDataTable("SELECT friendid AS remoteuserid, friendcategory AS remotecategory, usercategory AS localcategory FROM messenger_friendships WHERE userid = @userid  AND friendid > 0");
                FriendsRecv = dbClient.ReadDataTable("SELECT userid AS removeuserid, usercategory AS remotecategory, friendcategory AS localcategory FROM messenger_friendships WHERE friendid = @userid  AND userid > 0");
                Categories = dbClient.ReadDataTable("SELECT id, name FROM messenger_categories WHERE userid = @userid");
            }
            #endregion

            #region Loops
            lock (this)
            {
                for (int i = 0; i < Categories.Rows.Count; i++)
                {
                    this.fCategories.Add((new Category((uint)Categories.Rows[i]["id"], (string)Categories.Rows[i]["name"])));
                }
                for (int i = 0; i < FriendsSent.Rows.Count; i++)
                {
                    this.fFriends.Add((uint)FriendsSent.Rows[i]["remoteuserid"], new Friend(Core.GetUserDistributor().GetUser((uint)FriendsSent.Rows[i]["remoteuserid"]), (uint)FriendsSent.Rows[i]["remotecategory"], (uint)FriendsSent.Rows[i]["localcategory"]));
                }
                for (int i = 0; i < FriendsRecv.Rows.Count; i++)
                {
                    this.fFriends.Add((uint)FriendsRecv.Rows[i]["remoteuserid"], new Friend(Core.GetUserDistributor().GetUser((uint)FriendsRecv.Rows[i]["remoteuserid"]), (uint)FriendsRecv.Rows[i]["remotecategory"], (uint)FriendsSent.Rows[i]["localcategory"]));
                }
            }
            #endregion
        }
        
        /// <summary>
        /// Add an update to be send in the next SendUpdates() call.
        /// The updates are stored in the update buffer.
        /// </summary>
        /// <param name="Update">The data to send.</param>
        internal Messenger AddUpdate(FriendUpdate Update)
        {
            this.fUpdateList.Add(Update);

            return this;
        }

        /// <summary>
        /// Send the initial (at logon) list of friends and categories.
        /// </summary>
        internal Messenger SendInitialList()
        {
            Friend[] Friends = new Friend[this.fFriends.Count];

            this.fFriends.Values.CopyTo(Friends, 0);

            this.fUser.GetPacketSender().Send_MessengerInit(200, 200, 600, this.fCategories.ToArray(), Friends, 200);

            return this;
        }

        /// <summary>
        /// Send a list of all unanswered friend requests recieved.
        /// This is normally sent at logon.
        /// </summary>
        internal Messenger SendFriendRequests()
        {
            #region Database Stuff
            DataTable dTable;

            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("userid", this.fUser.GetID());
                dTable = dbClient.ReadDataTable("SELECT messenger_friendrequests.userid_from, users.name FROM messenger_friendrequests, users WHERE messenger_friendrequests.userid_to = @userid AND messenger_friendrequests.userid_to > 0 AND messenger_friendrequests.userid_from > 0 AND messenger_friendrequests.userid_from = users.id ORDER BY users.name ASC");
            }
            #endregion

            uint[] UserIDs = new uint[dTable.Rows.Count];
            string[] Usernames = new string[UserIDs.Length];

            for (int i = 0; i < UserIDs.Length; i++)
            {
                UserIDs[i] = (uint)dTable.Rows[i]["userid_from"];
                Usernames[i] = (string)dTable.Rows[i]["name"];
            }

            this.fUser.GetPacketSender().Send_FriendRequests(UserIDs, Usernames);

            return this;
        }

        /// <summary>
        /// Send all updates in the update buffer to the client.
        /// The buffer is cleared afterwards.
        /// </summary>
        internal Messenger SendUpdates()
        {
            Friend[] Friends = new Friend[this.fFriends.Count];

            this.fFriends.Values.CopyTo(Friends, 0);

            this.fUser.GetPacketSender().Send_FriendListUpdate(this.fCategories.ToArray(), Friends, this.fUpdateList.ToArray());

            this.fUpdateList.Clear();

            return this;
        }

        /// <summary>
        /// Checks if a given user is friends with this user.
        /// </summary>
        /// <param name="RequireLoggedIn">If true then the user must also be online.</param>
        /// <returns>True if the user is on the friends list (and online if required), false otherwise.</returns>
        public bool IsFriendsWith(User User, bool RequireLoggedIn)
        {
            if (!this.fFriends.ContainsKey(User.GetID()))
                return false;
            if (RequireLoggedIn)
                return User.IsLoggedIn();
            return true;
        }

        /// <summary>
        /// Checks if a given user has requested to be friends with this user.
        /// </summary>
        /// <returns>Returns true if a friend request exists, false otherwise.</returns>
        public bool HasRequestFrom(User User)
        {
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("localuserid", this.fUser.GetID());
                dbClient.AddParamWithValue("remoteuserid", User.GetID());
                return dbClient.ReadExist("SELECT 0 FROM messenger_friendrequests WHERE (userid_to = @me AND userid_from = @them AND userid_to > 0 AND userid_from > 0) OR (userid_to = @them AND userid_from = @me AND userid_to > 0 AND userid_from > 0)");
            }
        }
        #endregion
    }

    public static class FriendUpdateActions
    {
        public const sbyte Remove = -1;
        public const sbyte NoChange = 0;
        public const sbyte Add = 1;
    }
}