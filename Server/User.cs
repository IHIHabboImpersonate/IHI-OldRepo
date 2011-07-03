using System;
using System.Collections.Generic;
using System.Data;

using Ion.Net.Connections;
using Ion.Storage;
using IHI.Server.Net.Messages;

namespace IHI.Server
{
    public class User
    {
        #region Constructors
        /// <summary>
        /// Construct a prelogin User object.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        internal User(IonTcpConnection Connection)
        {
            this.fConnection = Connection;
            this.fPacketSender = new PacketSender(this);
            this.fPacketProcessor = new PacketProcessor(this);
        }
        /// <summary>
        /// Construct a User object for the user with the given ID.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        /// <param name="ID">The user ID of user you wish to a User object for.</param>
        internal User(uint ID)
        {
            this.fID = ID;

            DataRow userRow;
            DataTable LongTermValues;
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("userid", this.fID);
                userRow = dbClient.ReadDataRow("SELECT name FROM users WHERE id = @userid");

                LongTermValues = dbClient.ReadDataTable("SELECT name, value FROM users_data_values WHERE userid = @userid");
            }

            this.fUsername = (string)userRow["name"];
            this.fMotto = (string)userRow["motto"];
            this.fFigure = (string)userRow["figure"];
            this.fGender = (bool)userRow["gender"];
            this.fLastAccess = (DateTime)userRow["lastaccess"];

            this.fLongTermValues = new Dictionary<string, string>();
            foreach (DataRow LongTermValue in LongTermValues.Rows)
            {
                this.fLongTermValues.Add((string)LongTermValue["name"], (string)LongTermValue["value"]);
            }
        }
        /// <summary>
        /// Construct a User object for the user with the given ID.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        /// <param name="Username">The username of user you wish to a User object for.</param>
        internal User(string Username)
        {
            this.fUsername = Username;

            DataRow UserRow;
            DataTable LongTermValues;
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("username", this.fUsername);
                UserRow = dbClient.ReadDataRow("SELECT id FROM users WHERE name = @username");
                
                this.fID = (uint)UserRow["id"];

                dbClient.AddParamWithValue("userid", this.fID);
                LongTermValues = dbClient.ReadDataTable("SELECT name, value FROM users_data_values WHERE userid = @userid");
            }

            this.fMotto = (string)UserRow["motto"];
            this.fFigure = (string)UserRow["figure"];
            this.fGender = (bool)UserRow["gender"];
            this.fLastAccess = (DateTime)UserRow["lastaccess"];

            this.fLongTermValues = new Dictionary<string, string>();
            foreach (DataRow LongTermValue in LongTermValues.Rows)
            {
                this.fLongTermValues.Add((string)LongTermValue["name"], (string)LongTermValue["value"]);
            }
        }
        #endregion
        
        #region Universal
        #region Fields
        /// <summary>
        /// The user ID of the user.
        /// </summary>
        private uint fID;
        /// <summary>
        /// The username of the user.
        /// </summary>
        private string fUsername;
        /// <summary>
        /// The motto of the user.
        /// </summary>
        private string fMotto;
        /// <summary>
        /// The figure of the user.
        /// </summary>
        private string fFigure; // TODO: More structured?
        /// <summary>
        /// The gender of the user.
        /// Male = True
        /// Female = False
        /// </summary>
        private bool fGender;
        /// <summary>
        /// The date and time of the last successful logon.
        /// </summary>
        private DateTime fLastAccess;
        #endregion

        #region Methods
        public uint GetID()
        {
            return this.fID;
        }
        public string GetUsername()
        {
            return this.fUsername;
        }
        public string GetMotto()
        {
            return this.fMotto;
        }
        public string GetFigure()
        {
            return this.fFigure;
        }
        public bool GetGender()
        {
            return this.fGender;
        }
        public DateTime GetLastAccess()
        {
            return this.fLastAccess;
        }
        #endregion
        #endregion

        #region Offline (Example: Friends List)
        #region Fields
        /// <summary>
        /// Stores session values for the user.
        /// These are saved to the database under user_data_values.
        /// </summary>
        private Dictionary<string, string> fLongTermValues;
        #endregion
        #endregion

        #region Online (Example: Local User)
        #region Fields
        /// <summary>
        /// The amount of times StartOnlineValues() has been called
        /// minus the amount of times StopOlineValues() has been called.
        /// </summary>
        private uint OnlineValueCounter = 0;
      
        /// <summary>
        /// Is the user logged in or is it a PreLoginUser?
        /// </summary>
        private bool fLoggedIn = false;

        #region Connection Related
        /// <summary>
        /// The current connection of this user.
        /// </summary>
        private IonTcpConnection fConnection;
        /// <summary>
        /// True if the user replied to the last ping?
        /// </summary>
        private bool fPonged;
        /// <summary>
        /// The packet sender for this user.
        /// </summary>
        private PacketSender fPacketSender;
        /// <summary>
        /// The packet sender for this user.
        /// </summary>
        private PacketProcessor fPacketProcessor;
        #endregion

        /// <summary>
        /// A string array of fuserights the user has.
        /// </summary>
        private string[] fFuseRights;
        /// <summary>
        /// Stores session values for the user.
        /// These are not saved when the user disconnects.
        /// </summary>
        private Dictionary<object, object> fSessionValues;
        #endregion

        #region Methods
        /// <summary>
        /// Ensure that the online values are present.
        /// ALWAYS CALL StopLoggedInValues() WHEN YOU ARE FINISHED!
        /// </summary>
        public void StartLoggedInValues(IonTcpConnection Connection)
        {
            OnlineValueCounter++;
            if (OnlineValueCounter == 1)
            {
                this.fPonged = true;

                this.fFuseRights = Core.GetFuseManager().GetUserRights(this.fID);
                this.fSessionValues = new Dictionary<object, object>();
            }
        }
        /// <summary>
        /// Cleans up the online values when nobody is using them.
        /// </summary>
        public void StopLoggedInValues()
        {
            OnlineValueCounter--;
            if (OnlineValueCounter == 0)
            {
                this.fConnection = null;
                this.fPacketSender = null;
                this.fPacketProcessor = null;
                this.fSessionValues = null;
                this.fFuseRights = null;
            }
        }

        /// <summary>
        /// Is the user logged in?
        /// </summary>
        public bool IsLoggedIn()
        {
            return this.fLoggedIn;
        }
        /// <summary>
        /// Set if the user as logged in.
        /// This also updates the LastAccess time if required.
        /// </summary>
        /// <param name="Value">The user's new logged in status.</param>
        public User SetLoggedIn(bool Value)
        {
            if (!this.fLoggedIn && Value)
            {
                // TODO: Update LastAccessTime
            }

            this.fLoggedIn = Value;
            return this;
        }

        #region Connection Related
        public IonTcpConnection GetConnection()
        {
            return this.fConnection;
        }
        public PacketSender GetPacketSender()
        {
            return this.fPacketSender;
        }
        public User Pong()
        {
            this.fPonged = true;
            return this;
        }

        #endregion
        
        /// <summary>
        /// Load the fuserights from the database.
        /// </summary>
        private string[] GetRights()
        {
            DataColumn dCol;
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("id", this.fID); // User groups that have been specifically given to the user.
                dbClient.AddParamWithValue("global", uint.MaxValue); // User groups that are given by default.
                dCol = dbClient.ReadDataColumn("SELECT groupid FROM fuse_item WHERE type='user' AND (valueid=@id OR valueid=@global)"); // Get all the fuse groups this user is a member of
            }

            if (dCol == null) // Are they in any groups?
                return new string[0]; // No - Return no rights

            HashSet<string> Return = new HashSet<string>();

            HashSet<ushort> Groups = new HashSet<ushort>();

            for (int i = 0; i < dCol.Table.Rows.Count; i++) // For every group the user is a direct member of...
            {
                Groups.Add((ushort)dCol.Table.Rows[i]["groupid"]); // Add it to the collection of every group they user is in (including children).
                Groups.UnionWith(Core.GetFuseManager().GetGroup((ushort)dCol.Table.Rows[i]["groupid"]).GetChildren()); // And then add all child groups too.
            }

            foreach (ushort Group in Groups) // For every group in the collection...
            {
                Return.UnionWith(Core.GetFuseManager().GetGroup(Group).GetRights()); // Add the rights from it to the collection of rights the user has.
            }

            string[] ReturnArray = new string[Return.Count];
            Return.CopyTo(ReturnArray); // Convert the collection to a string array
            this.fFuseRights = ReturnArray; // Return the array.

            return ReturnArray;
        }
        /// <summary>
        /// 
        /// </summary>
        internal void Disconnect()
        {

        }
        #endregion
        #endregion

        #region Room (Example: User in a Room)
        #region Fields
        /// <summary>
        /// The room the user is currently in.
        /// </summary>
        private Room fRoom;
        /// <summary>
        /// The current position in the room the user is currently at.
        /// </summary>
        private Position fPosition;
        /// <summary>
        /// The desired position in the room of this user.
        /// </summary>
        private Position fDestination;
        #endregion

        #region Methods
        /// <summary>
        /// Returns the current position the user is at in the room.
        /// </summary>
        public Position GetPosition()
        {
            return this.fPosition;
        }
        /// <summary>
        /// Sets the current position the user is at in the room.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public User SetPosition(Position Position)
        {
            this.fPosition = Position;
            // TODO: Force room update
            return this;
        }
        /// <summary>
        /// Returns the desired position in the room of this user.
        /// </summary>
        public Position GetDestination()
        {
            return this.fDestination;
        }
        /// <summary>
        /// Sets the desired position in the room of this user.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public User SetDestination(Position Destination)
        {
            this.fDestination = Destination;
            return this;
        }
                
        /// <summary>
        /// Whisper a message from the User to another User in the room.
        /// </summary>
        /// <param name="Recipient">The User to recieve the message.</param>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        public User Whisper(User Recipient, string Message)
        {

            return this;
        }
        /// <summary>
        /// Say a message from the User to close Users in the room.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        public User Say(string Message)
        {

            return this;
        }
        /// <summary>
        /// Shout a message from the User to all Users in the room.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        public User Shout(string Message)
        {

            return this;
        }

        /// <summary>
        /// Send the current details of the User to the room.
        /// The user will poof when used.
        /// No changes to the User object are made.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public User Refresh()
        {

            return this;
        }
        /// <summary>
        /// Make the User wave for the default length of time.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        /// <remarks>Unsure on the naming as of yet</remarks>
        public User Wave()
        {

            return this;
        }
        /// <summary>
        /// Make the User dance.
        /// </summary>
        /// <param name="Style">The style of dance to use. 0 = Stop Dancing</param>
        /// <returns>The current User object. This allows chaining.</returns>
        public User SetDance(byte Style)
        {

            return this;
        }

        /// <summary>
        /// Returns the current room the user is in.
        /// If the user is not in a room null is returned.
        /// </summary>
        /// <returns></returns>
        public Room GetRoom()
        {
            return this.fRoom;
        }
        #endregion
        #endregion
    }
}
