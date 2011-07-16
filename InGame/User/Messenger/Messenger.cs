using System.Collections.Generic;
using System.Data;
using System.Linq;
using IHI.Server.SubPackets;
using IHI.Server.Habbos;

using NHibernate;

namespace IHI.Server.Messenger
{
    /// <summary>
    /// Handles everything to do with the Messenger/Friends List/Console.
    /// </summary>
    public class Messenger
    {
        #region Fields
        private Habbo fHabbo;
        private Dictionary<int, Friend> fFriends;          // Holds all friends of this user. (The key is the Friend's UserId)
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
        internal Messenger(Habbo User, byte BlockFlags)
        {
            this.fHabbo = User;
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
                if (this.fFriends.ContainsKey(Friend.GetHabbo().GetID()))    // Check if they are already a friend.
                    return this;                                            // They are, abort!
                this.fFriends.Add(Friend.GetHabbo().GetID(), Friend);        // Nope, not a friend. Add them.
            }

            AddUpdate(new FriendUpdate(FriendUpdateActions.Add, Friend));   // Add the friend to the update data.

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
                if (!this.fFriends.ContainsKey(Friend.GetHabbo().GetID()))   // Does the Friend exist in the friend list?
                    this.fFriends.Remove(Friend.GetHabbo().GetID());         // Yes, remove it.
            }

            SendUpdates();  // Force an early update.

            return this;
        }

        /// <summary>
        /// Load all friends and categories from the database.
        /// </summary>
        internal void LoadFriendList()
        {
            IList<Database.MessengerFriend> FriendsA;
            IList<Database.MessengerFriend> FriendsB;
            IList<Database.MessengerCategory> Categories;  // Categories to put the friends in.

            using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
            {
                FriendsA = DB.CreateCriteria<Database.MessengerFriend>()
                                .Add(new NHibernate.Criterion.EqPropertyExpression("habbo_a_id", this.fHabbo.GetID().ToString()))
                                .List<Database.MessengerFriend>();

                FriendsB = DB.CreateCriteria<Database.MessengerFriend>()
                                .Add(new NHibernate.Criterion.EqPropertyExpression("habbo_b_id", this.fHabbo.GetID().ToString()))
                                .List<Database.MessengerFriend>();

                Categories = DB.CreateCriteria<Database.MessengerCategory>()
                                    .Add(new NHibernate.Criterion.EqPropertyExpression("habbo_id", this.fHabbo.GetID().ToString()))
                                    .List<Database.MessengerCategory>();
                                    
            }

            foreach (Database.MessengerFriend Friend in FriendsA)
            {
                // TODO: Heavy optimisation (mapping).
                this.fFriends.Add(
                    Friend.habbo_a_id, 
                    new Friend(
                        CoreManager.GetCore().GetUserDistributor().GetUser(Friend.habbo_b_id), 
                        Friend.category_a_id, 
                        Friend.category_b_id));
            }
            foreach (Database.MessengerFriend Friend in FriendsB)
            {
                // TODO: Heavy optimisation. (mapping)
                this.fFriends.Add(
                    Friend.habbo_a_id,
                    new Friend(CoreManager.GetCore().GetUserDistributor().GetUser(Friend.habbo_a_id), 
                        Friend.category_b_id, 
                        Friend.category_a_id));
            }

            foreach (Database.MessengerCategory Category in Categories)
            {
                // TODO: Consider using IHI.Database.MessengerCategory instead.
                this.fCategories.Add(new Category(Category.category_id, Category.name));
            }
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

            // this.fUser.GetPacketSender().Send_MessengerInit(200, 200, 600, this.fCategories.ToArray(), Friends, 200); // TODO: Move to a plugin!

            return this;
        }

        /// <summary>
        /// Send a list of all unanswered friend requests received.
        /// This is normally sent at logon.
        /// </summary>
        internal Messenger SendFriendRequests()
        {
            Database.MessengerFriendRequest[] Requests;

            using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
            {
                Requests =  DB.CreateCriteria<Database.MessengerFriendRequest>()
                                .Add(new NHibernate.Criterion.EqPropertyExpression("habbo_to_id", this.fHabbo.GetID().ToString()))
                                .List<Database.MessengerFriendRequest>()
                                    .ToArray();
            }

            //this.fHabbo.GetPacketSender().Send_FriendRequests(Request);
            // TODO: Move to a plugin

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

            // this.fUser.GetPacketSender().Send_FriendListUpdate(this.fCategories.ToArray(), Friends, this.fUpdateList.ToArray()); // TODO: Move to a plugin!

            this.fUpdateList.Clear();

            return this;
        }

        /// <summary>
        /// Checks if a given user is friends with this user.
        /// </summary>
        /// <param name="RequireLoggedIn">If true then the user must also be online.</param>
        /// <returns>True if the user is on the friends list (and online if required), false otherwise.</returns>
        public bool IsFriendsWith(Habbo User, bool RequireLoggedIn)
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
        public bool HasRequestFrom(Habbo User)
        {/*
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("localuserid", this.fUser.GetID());
                dbClient.AddParamWithValue("remoteuserid", User.GetID());
                return dbClient.ReadExist("SELECT 0 FROM messenger_friendrequests WHERE (userid_to = @me AND userid_from = @them AND userid_to > 0 AND userid_from > 0) OR (userid_to = @them AND userid_from = @me AND userid_to > 0 AND userid_from > 0)");
            }*/
            return false;
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