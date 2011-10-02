using System;
using System.Collections.Generic;
using System.Linq;
using IHI.Database;
using IHI.Server.Networking;
using IHI.Server.Networking.Messages;
using IHI.Server.Rooms;
using NHibernate.Criterion;

namespace IHI.Server.Habbos
{
    public delegate void HabboEventHandler(object source, HabboEventArgs e);

    public class Habbo : Human, IBefriendable, IMessageable
    {
        #region Constructors

        /// <summary>
        /// Construct a prelogin User object.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        internal Habbo(IonTcpConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Construct a User object for the user with the given ID.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        /// <param name="id">The user ID of user you wish to a User object for.</param>
        internal Habbo(int id)
        {
            _id = id;
            Database.Habbo habboData;

            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                habboData = db.CreateCriteria<Database.Habbo>().
                    Add(Restrictions.Eq("habbo_id", id)).
                    SetProjection(Projections.ProjectionList().
                                      Add(Projections.Property("username"), "username").
                                      Add(Projections.Property("motto"), "motto").
                                      Add(Projections.Property("figure"), "figure").
                                      Add(Projections.Property("gender"), "gender")).
                    Add(Restrictions.Eq("habbo_id", id)).
                    List<Database.Habbo>().
                    First();
            }
            DisplayName = _username = habboData.username;
            _motto = habboData.motto;
            Figure = CoreManager.GetServerCore().GetHabboFigureFactory().Parse(habboData.figure, habboData.gender);
        }

        /// <summary>
        /// Construct a User object for the user with the given ID.
        /// DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        /// <param name="username">The username of user you wish to a User object for.</param>
        internal Habbo(string username)
        {
            _username = DisplayName = username;

            Database.Habbo habboData;

            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                habboData = db.CreateCriteria<Database.Habbo>()
                    .SetProjection(Projections.ProjectionList()
                                       .Add(Projections.Property("habbo_id"))
                                       .Add(Projections.Property("motto"))
                                       .Add(Projections.Property("figure"))
                                       .Add(Projections.Property("gender")))
                    .Add(new EqPropertyExpression("username", username))
                    .List<Database.Habbo>().First();
            }

            _id = habboData.habbo_id;
            _motto = habboData.motto;
            Figure = CoreManager.GetServerCore().GetHabboFigureFactory().Parse(habboData.figure, habboData.gender);
        }

        public Habbo(Database.Habbo habboData)
        {
            _id = habboData.habbo_id;

            DisplayName = _username = habboData.username;
            _motto = habboData.motto;
            Figure = CoreManager.GetServerCore().GetHabboFigureFactory().Parse(habboData.figure, habboData.gender);
        }

        #endregion

        #region Fields

        /// <summary>
        /// The user ID of the user.
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// The motto of the user.
        /// </summary>
        private readonly string _motto;

        /// <summary>
        /// The username of the user.
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// The amount of credits the user has.
        /// Set to null to update from the database next time the credits are accessed.
        /// </summary>
        private int? _creditBalance;

        /// <summary>
        /// The date and time of the last successful logon.
        /// </summary>
        private DateTime _lastAccess;

        /// <summary>
        /// Is the user logged in or is it a PreLoginUser?
        /// </summary>
        private bool _isLoggedIn;

        #region Permissions

        private HashSet<int> _permissions;

        #endregion

        #region Connection Related

        /// <summary>
        /// The current Connection of this user.
        /// </summary>
        private IonTcpConnection _connection;

        private Dictionary<string, object> _instanceVariables;

        /// <summary>
        /// True if the user replied to the last ping?
        /// </summary>
        private bool _ponged;

        #endregion

        public static event HabboEventHandler OnHabboLogin;

        #endregion

        #region Methods

        /// <summary>
        /// The ID of the User.
        /// </summary>
        public int GetID()
        {
            return _id;
        }

        /// <summary>
        /// The motto of the User.
        /// </summary>
        public string GetMotto()
        {
            return _motto;
        }

        /// <summary>
        /// The date of the User's last login.
        /// </summary>
        public DateTime GetLastAccess()
        {
            return _lastAccess;
        }

        /// <summary>
        /// Is the user logged in?
        /// </summary>
        public bool IsLoggedIn()
        {
            return _isLoggedIn;
        }

        /// <summary>
        /// The name of the User.
        /// </summary>
        public string GetUsername()
        {
            return _username;
        }

        /// <summary>
        /// Set if the user is logged in.
        /// This also updates the LastAccess time if required.
        /// </summary>
        /// <param name="value">The user's new logged in status.</param>
        public Habbo SetLoggedIn(bool value)
        {
            if (OnHabboLogin != null)
                OnHabboLogin.Invoke(this, new HabboEventArgs());
            HabboDistributor.InvokeHabboLoginEvent(this, new HabboEventArgs());

            if (!_isLoggedIn && value)
            {
                _lastAccess = DateTime.Now;
                using (var db = CoreManager.GetServerCore().GetDatabaseSession())
                {
                    var habbo = db.Get<Database.Habbo>(_id);
                    habbo.last_access = _lastAccess;
                    db.Update(habbo);
                }
            }

            _isLoggedIn = value;
            return this;
        }

        /// <summary>
        /// Returns the amount credits the user has.
        /// </summary>
        public int GetCreditBalance()
        {
            if (_creditBalance == null)
            {
                using (var db = CoreManager.GetServerCore().GetDatabaseSession())
                {
                    _creditBalance = db.CreateCriteria<Habbo>().
                        SetProjection(Projections.Property("credits")).
                        Add(new EqPropertyExpression("habbo_id", GetID().ToString())).
                        List<int>().First();
                }
            }

            return (int) _creditBalance;
        }

        /// <summary>
        /// Set the amount credits the user has.
        /// </summary>
        /// <param name="balance">The amount of credits.</param>
        public Habbo SetCreditBalance(int balance)
        {
            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                var habboData = db.CreateCriteria<Habbo>().
                    SetProjection(Projections.Property("credits")).
                    Add(new EqPropertyExpression("habbo_id", GetID().ToString())).
                    List<Database.Habbo>().First();

                _creditBalance = habboData.credits = balance;

                db.Update(habboData);
            }
            return this;
        }

        /// <summary>
        /// Adds credits to the user's balance.
        /// </summary>
        /// <param name="amount">The amount of credits.</param>
        public Habbo GiveCredits(int amount)
        {
            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                var habboData = db.CreateCriteria<Habbo>().
                    SetProjection(Projections.Property("credits")).
                    Add(new EqPropertyExpression("habbo_id", GetID().ToString())).
                    List<Database.Habbo>().First();

                _creditBalance = habboData.credits += amount;

                db.Update(habboData);
            }
            return this;
        }

        /// <summary>
        /// Deduct credits from the user's balance.
        /// </summary>
        /// <param name="amount">The amount of credits.</param>
        public Habbo TakeCredits(int amount)
        {
            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                var habboData = db.CreateCriteria<Habbo>().
                    SetProjection(Projections.Property("credits")).
                    Add(new EqPropertyExpression("habbo_id", GetID().ToString())).
                    List<Database.Habbo>().First();

                _creditBalance = habboData.credits -= amount;

                db.Update(habboData);
            }
            return this;
        }

        #region Connection Related

        public IonTcpConnection GetConnection()
        {
            return _connection;
        }

        public Habbo Pong()
        {
            _ponged = true;
            return this;
        }
        public bool HasPonged()
        {
            return _ponged;
        }

        public void LoginMerge(Habbo loggedInUser)
        {
            _connection = loggedInUser._connection;

            _connection.Habbo = this;
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
            _permissions = new HashSet<int>(CoreManager.GetServerCore().GetPermissionManager().GetHabboPermissions(_id));
            return this;
        }

        /// <summary>
        /// Check if this User has PermissionID.
        /// </summary>
        public bool HasPermission(int permissionID)
        {
            if (_permissions == null)
                ReloadPermissions();
            return _permissions.Contains(permissionID);
        }

        /// <summary>
        /// Returns all PermissionIDs that this User has.
        /// </summary>
        public int[] GetPermissions()
        {
            if (_permissions == null)
                ReloadPermissions();
            lock (_permissions)
            {
                var permissionArray = new int[_permissions.Count];

                _permissions.CopyTo(permissionArray);
                return permissionArray;
            }
        }

        #endregion

        #endregion

        #endregion

        #region IBefriendable Members

        public object GetInstanceVariable(string name)
        {
            if (_instanceVariables == null || !_instanceVariables.ContainsKey(name))
                return null;
            return _instanceVariables[name];
        }

        public IInstanceVariables SetInstanceVariable(string name, object value)
        {
            if (_instanceVariables == null)
                _instanceVariables = new Dictionary<string, object>();

            if (_instanceVariables.ContainsKey(name))
                _instanceVariables[name] = value;
            else
                _instanceVariables.Add(name, value);

            return this;
        }


        public string GetPersistantVariable(string name)
        {
            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                var variables = (List<PersistantVariableHabbo>) db.CreateCriteria<PersistantVariableHabbo>().
                                                                    Add(Restrictions.Eq("habbo_id", GetID())).
                                                                    Add(Restrictions.Eq("variable_name", name)).
                                                                    List<PersistantVariableHabbo>();
                if (variables.Count != 0)
                    return variables[0].variable_value;
                return null;
            }
        }

        public IPersistantVariables SetPersistantVariable(string name, string value)
        {
            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                var variables = (List<PersistantVariableHabbo>) db.CreateCriteria<PersistantVariableHabbo>().
                                                                    Add(Restrictions.Eq("habbo_id", GetID())).
                                                                    Add(Restrictions.Eq("variable_name", name)).
                                                                    List<PersistantVariableHabbo>();
                if (variables.Count != 0)
                {
                    variables[0].variable_value = value;
                    db.SaveOrUpdate(variables[0]);
                    return this;
                }

                var newVariable = new PersistantVariableHabbo {variable_name = name, variable_value = value};

                db.SaveOrUpdate(newVariable);

                return this;
            }
        }

        #endregion

        #region IMessageable Members

        public IMessageable SendMessage(IInternalOutgoingMessage message)
        {
            _connection.SendMessage(message);
            return this;
        }

        #endregion
    }

    public class HabboEventArgs : EventArgs
    {
    }
}