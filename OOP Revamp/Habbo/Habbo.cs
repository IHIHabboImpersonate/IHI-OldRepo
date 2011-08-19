using System;
using System.Collections.Generic;
using System.Data;
using IHI.Server.Networking.Messages;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using System.Linq;
using IHI.Database;
using NHibernate;

using IHI.Server.Rooms;

namespace IHI.Server.Habbos
{
    public delegate void HabboEventHandler(object source, UserEventArgs e);

    public class Habbo : Human, IBefriendable, IMessageable, IObjectVariables
    {
        #region Constructors
        /// <summary>
        /// Construct a prelogin User object.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        internal Habbo(IonTcpConnection Connection)
        {
            this.fConnection = Connection;
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

            using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
            {
                HabboData = DB.Get<Database.Habbo>(ID); // TODO: Heavily optimise (mapping).
            }
            this.fDisplayName = this.fUsername = HabboData.username;
            this.fMotto = HabboData.motto;
            this.fFigure = CoreManager.GetCore().GetHabboFigureFactory().Parse(HabboData.figure, HabboData.gender);
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

            using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
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
            this.fFigure = CoreManager.GetCore().GetHabboFigureFactory().Parse(HabboData.figure, HabboData.gender);
        }
        #endregion

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
        private string fMotto;
        /// <summary>
        /// The date and time of the last successful logon.
        /// </summary>
        private DateTime fLastAccess;
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

        private Dictionary<string, object> fObjectVariables;
        #endregion


        /// <summary>
        /// The amount of credits the user has.
        /// </summary>
        private int? fCreditBalance;

        #region Permissions
        private HashSet<int> fPermissions;
        #endregion

        public static event HabboEventHandler OnHabboLogin;
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
        public DateTime GetLastAccess()
        {
            return this.fLastAccess;
        }

        /// <summary>
        /// Is the user logged in?
        /// </summary>
        public bool IsLoggedIn()
        {
            return this.fLoggedIn;
        }
        /// <summary>
        /// Set if the user is logged in.
        /// This also updates the LastAccess time if required.
        /// </summary>
        /// <param name="Value">The user's new logged in status.</param>
        public Habbo SetLoggedIn(bool Value)
        {
            if (!this.fLoggedIn && Value)
            {
                this.fLastAccess = DateTime.Now;
                using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
                {
                    Database.Habbo H = DB.Get<Database.Habbo>(this.fID);
                    H.last_access = this.fLastAccess;
                    DB.Update(H);
                }
                if(OnHabboLogin != null)
                    OnHabboLogin.Invoke(this, new UserEventArgs());
            }

            this.fLoggedIn = Value;
            return this;
        }
        /// <summary>
        /// Returns the amount credits the user has.
        /// </summary>
        public int GetCreditBalance()
        {
            if(this.fCreditBalance == null)
            {
                using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
                {
                    // TODO: Heavily optimise (mapping).
                    this.fCreditBalance = DB.Get<Database.Habbo>(this.GetID()).credits;
                }
            }

            return (int)this.fCreditBalance;
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
            if(this.fCreditBalance == null)
            {
                using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
                {
                    // TODO: Heavily optimise (mapping).
                    this.fCreditBalance = DB.Get<Database.Habbo>(this.GetID()).credits;
                }
            }
            this.fCreditBalance += Amount;
            return this;
        }
        /// <summary>
        /// Deduct credits from the user's balance.
        /// </summary>
        /// <param name="Amount">The amount of credits.</param>
        public Habbo TakeCredits(int Amount)
        {
            if(this.fCreditBalance == null)
            {
                using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
                {
                    // TODO: Heavily optimise (mapping).
                    this.fCreditBalance = DB.Get<Database.Habbo>(this.GetID()).credits;
                }
            }

            this.fCreditBalance -= Amount;
            return this;
        }

        #region Connection Related
        public IonTcpConnection GetConnection()
        {
            return this.fConnection;
        }
        public Habbo Pong()
        {
            this.fPonged = true;
            return this;
        }

        public void LoginMerge(Habbo LoggedInUser)
        {
            this.fConnection = LoggedInUser.fConnection;
            
            this.fConnection.fUser = this;
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
            this.fPermissions = new HashSet<int>(CoreManager.GetCore().GetPermissionManager().GetHabboPermissions(this.fID));
            return this;
        }

        /// <summary>
        /// Check if this User has PermissionID.
        /// </summary>
        public bool HasPermission(int PermissionID)
        {
            if(this.fPermissions == null)
                this.ReloadPermissions();
            return this.fPermissions.Contains(PermissionID);
        }

        /// <summary>
        /// Returns all PermissionIDs that this User has.
        /// </summary>
        public int[] GetPermissions()
        {
            if(this.fPermissions == null)
                this.ReloadPermissions();
            lock (this.fPermissions)
            {
                int[] PermissionArray = new int[this.fPermissions.Count];

                this.fPermissions.CopyTo(PermissionArray);
                return PermissionArray;
            }
        }
        #endregion
        #endregion
        #endregion
        
        public IMessageable SendMessage(IInternalOutgoingMessage Message)
        {
            this.fConnection.SendMessage(Message);
            return this;
        }

        public object GetInstanceVariable(string Name)
        {
            if (this.fObjectVariables == null || !this.fObjectVariables.ContainsKey(Name))
                return null;
            return this.fObjectVariables[Name];
        }

        public IObjectVariables SetInstanceVariable(string Name, object Value)
        {
            if (this.fObjectVariables == null)
                this.fObjectVariables = new Dictionary<string, object>();

            if (this.fObjectVariables.ContainsKey(Name))
                this.fObjectVariables[Name] = Value;
            else
                this.fObjectVariables.Add(Name, Value);

            return this;
        }


        public string GetPersistantVariable(string Name)
        {
            throw new NotImplementedException();
        }

        public IObjectVariables SetPersistantVariable(string Name, string Value)
        {
            throw new NotImplementedException();
        }
    }

    public class UserEventArgs : EventArgs
    {

    }
}