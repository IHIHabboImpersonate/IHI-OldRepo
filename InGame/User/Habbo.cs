using System;
using System.Collections.Generic;
using System.Data;
using IHI.Server.Networking.Messages;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using System.Linq;
using IHI.Database;
using NHibernate;


namespace IHI.Server.Habbos
{
    public delegate void HabboEventHandler(object sender, UserEventArgs e);

    public class Habbo : IHI.Server.Human
    {
        #region Constructors
        /// <summary>
        /// Construct a prelogin User object.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        internal Habbo(IonTcpConnection Connection)
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
        internal Habbo(int ID)
        {
            this.fID = ID;
            Database.Habbo HabboData;

            using (ISession DB = Core.GetDatabaseSession())
            {
                HabboData = DB.Get<Database.Habbo>(ID); // TODO: Heavily optimise (mapping).
            }

            this.fUsername = this.fDisplayName = HabboData.username;
            this.fMotto = HabboData.motto;
            this.fFigure = HabboData.figure;
            this.fGender = HabboData.gender;
        }
        /// <summary>
        /// Construct a User object for the user with the given ID.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        /// <param name="Username">The username of user you wish to a User object for.</param>
        internal Habbo(string Username)
        {
            this.fUsername = this.fDisplayName = Username;

            Database.Habbo HabboData;

            using (ISession DB = Core.GetDatabaseSession())
            {
                HabboData = DB.CreateCriteria<Database.Habbo>()
                                .SetProjection(NHibernate.Criterion.Projections.ProjectionList()
                                    .Add(NHibernate.Criterion.Projections.Property("habbo_id"))
                                    .Add(NHibernate.Criterion.Projections.Property("motto"))
                                    .Add(NHibernate.Criterion.Projections.Property("figure"))
                                    .Add(NHibernate.Criterion.Projections.Property("gender")))
                                .Add(new NHibernate.Criterion.EqPropertyExpression("username", Username))
                                .List<Database.Habbo>().First();
            }

            this.fID = HabboData.habbo_id;
            this.fMotto = HabboData.motto;
            this.fFigure = HabboData.figure;
            this.fGender = HabboData.gender;
        }
        #endregion

        #region Universal
        #region Fields
        /// <summary>
        /// The user ID of the user.
        /// </summary>
        private int fID;
        /// <summary>
        /// The username of the user.
        /// </summary>
        private string fUsername;
        /// <summary>
        /// The motto of the user.
        /// </summary>
        private string fMotto; // TODO: More structured?
        /// <summary>
        /// The date and time of the last successful logon.
        /// </summary>
        private DateTimeOffset fLastAccess;
        /// <summary>
        /// Stores session values for the user.
        /// These are saved to the database under user_data_values.
        /// </summary>
        private Dictionary<string, string> fLongTermValues;
        #endregion

        #region Methods
        /// <summary>
        /// The ID of the User.
        /// </summary>
        public int GetID()
        {
            return this.fID;
        }
        /// <summary>
        /// The name of the User.
        /// </summary>
        public string GetUsername()
        {
            return this.fUsername;
        }
        /// <summary>
        /// The motto of the User.
        /// </summary>
        public string GetMotto()
        {
            return this.fMotto;
        }
        /// <summary>
        /// The date of the User's last login.
        /// </summary>
        public DateTimeOffset GetLastAccess()
        {
            return this.fLastAccess;
        }
        #endregion
        #endregion

        #region Online (Example: Local User, Permission Access)
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
        /// The current Connection of this user.
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
        /// Stores session values for the user.
        /// These are not saved when the user disconnects.
        /// </summary>
        private Dictionary<object, object> fSessionValues;

        /// <summary>
        /// The amount of credits the user has.
        /// </summary>
        private int fCreditBalance;

        #region Permissions
        private HashSet<int> fPermissions;
        #endregion

        public event HabboEventHandler OnUserLogin;
        #endregion

        #region Methods
        /// <summary>
        /// Ensure that the online values are present.
        /// ALWAYS CALL StopLoggedInValues() WHEN YOU ARE FINISHED!
        /// </summary>
        public void StartLoggedInValues()
        {
            OnlineValueCounter++;
            if (OnlineValueCounter == 1)
            {
                using (ISession DB = Core.GetDatabaseSession())
                {
                    // TODO: Heavily optimise (mapping).
                    this.fCreditBalance = DB.Get<Database.Habbo>(this.GetID()).credits;
                }

                this.fPonged = true;
                this.fSessionValues = new Dictionary<object, object>();

                this.ReloadPermissions();
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
        public Habbo SetLoggedIn(bool Value)
        {
            if (!this.fLoggedIn && Value)
            {
                this.fLastAccess = DateTimeOffset.Now;
                using (ISession DB = Core.GetDatabaseSession())
                {
                    DB.Update(DB.Get<Database.Habbo>(this.fID).last_access = this.fLastAccess);
                }
            }

            this.fLoggedIn = Value;
            return this;
        }
        /// <summary>
        /// Returns the amount credits the user has.
        /// </summary>
        public int GetCreditBalance()
        {
            return this.fCreditBalance;
        }
        /// <summary>
        /// Set the amount credits the user has.
        /// </summary>
        /// <param name="Balance">The amount of credits.</param>
        public Habbo SetCreditBalance(int Balance)
        {
            this.fCreditBalance = Balance;
            return this;
        }
        /// <summary>
        /// Adds credits to the user's balance.
        /// </summary>
        /// <param name="Amount">The amount of credits.</param>
        public Habbo GiveCredits(int Amount)
        {
            this.fCreditBalance += Amount;
            return this;
        }
        /// <summary>
        /// Deduct credits from the user's balance.
        /// </summary>
        /// <param name="Amount">The amount of credits.</param>
        public Habbo TakeCredits(int Amount)
        {
            this.fCreditBalance -= Amount;
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
        public Habbo Pong()
        {
            this.fPonged = true;
            return this;
        }

        public void LoginMerge(Habbo LoggedInUser)
        {
            this.fConnection = LoggedInUser.fConnection;
            this.fPacketProcessor = LoggedInUser.fPacketProcessor;
            this.fPacketSender = LoggedInUser.fPacketSender;

            this.fConnection.fUser = this;
            this.fPacketProcessor.fUser = this;
            this.fPacketSender.fUser = this;
        }
        #endregion

        #region Permissions
        #region IHI Permission System
        /// <summary>
        /// Reloads the permissions for the User.
        /// </summary>
        /// <returns>The User the permissions were reloaded for. This is for Chaining.</returns>
        public Habbo ReloadPermissions()
        {
            this.fPermissions = new HashSet<int>(Core.GetPermissionManager().GetHabboPermissions(this.fID));
            return this;
        }

        /// <summary>
        /// Check if this User has PermissionID.
        /// </summary>
        public bool HasPermission(int PermissionID)
        {
            return this.fPermissions.Contains(PermissionID);
        }

        /// <summary>
        /// Returns all PermissionIDs that this User has.
        /// </summary>
        public int[] GetPermissions()
        {
            lock (this.fPermissions)
            {
                int[] PermissionArray = new int[this.fPermissions.Count];

                this.fPermissions.CopyTo(PermissionArray);
                return PermissionArray;
            }
        }
        #endregion
        #endregion

        public object GetSessionValue(object Key)
        {
            if (this.fSessionValues.ContainsKey(Key))
                return this.fSessionValues[Key];
            return null;
        }
        public object SetSessionValue(object Key, object Value)
        {
            if (this.fSessionValues.ContainsKey(Key))
                this.fSessionValues[Key] = Value;
            else
                this.fSessionValues.Add(Key, Value);
            return null;
        }
        public bool IsSessionValuePresent(object Key)
        {
            return this.fSessionValues.ContainsKey(Key);
        }
        #endregion
        #endregion

        #region Room (Example: User in a fRoom)
        #region Fields

        #endregion

        #region Methods
        #endregion
        #endregion
    }

    public class UserEventArgs : EventArgs
    {

    }
}