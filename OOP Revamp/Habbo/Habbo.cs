#region GPLv3

// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using IHI.Database;
using IHI.Server.Networking;
using IHI.Server.Networking.Messages;
using IHI.Server.Rooms;
using NHibernate;
using NHibernate.Criterion;

#endregion

namespace IHI.Server.Habbos
{
    public delegate void HabboEventHandler(object source, HabboEventArgs e);

    public class Habbo : Human, IBefriendable, IMessageable
    {
        #region Constructors

        /// <summary>
        ///   Construct a prelogin User object.
        ///   DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        internal Habbo(IonTcpConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        ///   Construct a User object for the user with the given ID.
        ///   DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        /// <param name = "id">The user ID of user you wish to a User object for.</param>
        internal Habbo(int id)
        {
            _id = id;
            Database.Habbo habboData;

            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                habboData = db.Get<Database.Habbo>(id);
            }
            DisplayName = _username = habboData.username;
            _motto = habboData.motto;
            Figure = CoreManager.ServerCore.GetHabboFigureFactory().Parse(habboData.figure, habboData.gender);
        }

        /// <summary>
        ///   Construct a User object for the user with the given ID.
        ///   DO NOT USE THIS FOR GETTING A USER - USE THE USER DISTRIBUTOR
        /// </summary>
        /// <param name = "username">The username of user you wish to a User object for.</param>
        internal Habbo(string username)
        {
            _username = DisplayName = username;
            Database.Habbo habbo = new Database.Habbo
                                       {
                                           username = username
                                       };
            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                habbo = db.CreateCriteria<Database.Habbo>()
                    .Add(Example.Create(habbo))
                    .UniqueResult<Database.Habbo>();
            }

            _id = habbo.habbo_id;
            _motto = habbo.motto;
            Figure = CoreManager.ServerCore.GetHabboFigureFactory().Parse(habbo.figure, habbo.gender);
        }

        public Habbo(Database.Habbo habboData)
        {
            _id = habboData.habbo_id;

            DisplayName = _username = habboData.username;
            _motto = habboData.motto;
            Figure = CoreManager.ServerCore.GetHabboFigureFactory().Parse(habboData.figure, habboData.gender);
        }

        #endregion

        #region Fields

        /// <summary>
        ///   The user ID of the user.
        /// </summary>
        private readonly int _id;

        /// <summary>
        ///   The motto of the user.
        /// </summary>
        private readonly string _motto;

        /// <summary>
        ///   The username of the user.
        /// </summary>
        private readonly string _username;

        /// <summary>
        ///   The amount of credits the user has.
        ///   Set to null to update from the database next time the credits are accessed.
        /// </summary>
        private int? _creditBalance;

        /// <summary>
        ///   Is the user logged in or is it a PreLoginUser?
        /// </summary>
        private bool _isLoggedIn;

        /// <summary>
        ///   The date and time of the last successful logon.
        /// </summary>
        private DateTime _lastAccess;

        #region Permissions

        private HashSet<string> _fusePermissions;
        private HashSet<int> _permissions;
        private HashSet<string> _sentFusePermissions;

        #endregion

        #region Connection Related

        /// <summary>
        ///   The current Connection of this user.
        /// </summary>
        private IonTcpConnection _connection;

        private Dictionary<string, object> _instanceVariables;

        /// <summary>
        ///   True if the user replied to the last ping?
        /// </summary>
        private bool _ponged;

        #endregion

        #region Events

        public event MessengerBlockFlagEventHandler OnBlockFlagChanged;
        public event HabboEventHandler OnPreHabboLogin;
        public event HabboEventHandler OnHabboLogin;
        public event HabboEventHandler OnRebuildFusePermissions;

        #endregion

        public bool BlockStalking { get; set; }
        public bool BlockRequests { get; set; }
        public bool BlockInvites { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///   The ID of the User.
        /// </summary>
        public int GetID()
        {
            return _id;
        }

        /// <summary>
        ///   The motto of the User.
        /// </summary>
        public string GetMotto()
        {
            return _motto;
        }

        /// <summary>
        ///   The date of the User's last login.
        /// </summary>
        public DateTime GetLastAccess()
        {
            return _lastAccess;
        }

        /// <summary>
        ///   Is the user logged in?
        /// </summary>
        public bool IsLoggedIn()
        {
            return _isLoggedIn;
        }

        /// <summary>
        ///   The name of the User.
        /// </summary>
        public string GetUsername()
        {
            return _username;
        }

        /// <summary>
        ///   Set if the user is logged in.
        ///   This also updates the LastAccess time if required.
        /// </summary>
        /// <param name = "value">The user's new logged in status.</param>
        public Habbo SetLoggedIn(bool value)
        {
            if (!_isLoggedIn && value)
            {
                HabboEventArgs habboEventArgs = new HabboEventArgs();
                if (OnPreHabboLogin != null)
                    OnPreHabboLogin(this, habboEventArgs);

                CoreManager.ServerCore.GetHabboDistributor().InvokeOnPreHabboLogin(this, habboEventArgs);
                if (habboEventArgs.Cancelled)
                {
                    GetConnection().Disconnect();
                    return this;
                }

                _lastAccess = DateTime.Now;
                using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
                {
                    Database.Habbo habbo = db.Get<Database.Habbo>(_id);
                    habbo.last_access = _lastAccess;
                    db.Update(habbo);
                }
            }

            _isLoggedIn = value;
            return this;
        }

        #region Credits

        /// <summary>
        ///   Returns the amount credits the user has.
        /// </summary>
        public int GetCreditBalance()
        {
            if (!_creditBalance.HasValue)
            {
                using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
                {
                    _creditBalance = db.CreateCriteria<Habbo>()
                        .SetProjection(Projections.Property("credits"))
                        .Add(new EqPropertyExpression("habbo_id", GetID().ToString()))
                        .UniqueResult<int>();
                }
            }

            return _creditBalance.Value;
        }

        /// <summary>
        ///   Set the amount credits the user has.
        /// </summary>
        /// <param name = "balance">The amount of credits.</param>
        public Habbo SetCreditBalance(int balance)
        {
            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                Database.Habbo habboData = db.CreateCriteria<Habbo>()
                    .SetProjection(Projections.Property("credits"))
                    .Add(new EqPropertyExpression("habbo_id", _id.ToString()))
                    .UniqueResult<Database.Habbo>();

                _creditBalance = habboData.credits = balance;

                db.Update(habboData);
            }
            return this;
        }

        /// <summary>
        ///   Adds credits to the user's balance.
        /// </summary>
        /// <param name = "amount">The amount of credits.</param>
        public Habbo GiveCredits(int amount)
        {
            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                Database.Habbo habboData = db.CreateCriteria<Habbo>()
                    .SetProjection(Projections.Property("credits"))
                    .Add(new EqPropertyExpression("habbo_id", _id.ToString()))
                    .UniqueResult<Database.Habbo>();

                _creditBalance = habboData.credits += amount;

                db.Update(habboData);
            }
            return this;
        }

        /// <summary>
        ///   Deduct credits from the user's balance.
        /// </summary>
        /// <param name = "amount">The amount of credits.</param>
        public Habbo TakeCredits(int amount)
        {
            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                Database.Habbo habboData = db.CreateCriteria<Habbo>()
                    .SetProjection(Projections.Property("credits"))
                    .Add(new EqPropertyExpression("habbo_id", _id.ToString()))
                    .UniqueResult<Database.Habbo>();

                _creditBalance = habboData.credits -= amount;

                db.Update(habboData);
            }
            return this;
        }

        #endregion

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
        ///   Reloads the permissions for the User.
        /// </summary>
        /// <returns>The User the permissions were reloaded for. This is for Chaining.</returns>
        public Habbo ReloadPermissions()
        {
            _permissions = new HashSet<int>(CoreManager.ServerCore.GetPermissionManager().GetHabboPermissions(_id));
            return this;
        }

        /// <summary>
        ///   Check if this User has PermissionID.
        /// </summary>
        public bool HasPermission(int permissionID)
        {
            if (_permissions == null)
                ReloadPermissions();
            return _permissions.Contains(permissionID);
        }

        /// <summary>
        ///   Returns all PermissionIDs that this User has.
        /// </summary>
        public int[] GetPermissions()
        {
            if (_permissions == null)
                ReloadPermissions();
            lock (_permissions)
            {
                int[] permissionArray = new int[_permissions.Count];

                _permissions.CopyTo(permissionArray);
                return permissionArray;
            }
        }

        #endregion

        #region Fuse Permission System

        public Habbo GiveFusePermission(string fusePermission)
        {
            _fusePermissions.Add(fusePermission);
            return this;
        }

        public Habbo RebuildFusePermissions()
        {
            _fusePermissions = new HashSet<string>();
            if (_sentFusePermissions == null)
                _sentFusePermissions = new HashSet<string>();

            if (OnRebuildFusePermissions != null)
                OnRebuildFusePermissions(this, new HabboEventArgs());

            return this;
        }

        public IEnumerable<string> GetFusePermissions(bool excludeSent = false)
        {
            if (_fusePermissions == null)
                RebuildFusePermissions();
            if (!excludeSent)
                return _fusePermissions;
            return _fusePermissions.Except(_sentFusePermissions);
        }

        public Habbo SetFusePermissionSent(string fusePermission, bool sent = true)
        {
            if (sent)
                _sentFusePermissions.Add(fusePermission);
            else
                _sentFusePermissions.Remove(fusePermission);
            return this;
        }

        public Habbo SetFusePermissionSent(IEnumerable<string> fusePermissions, bool sent = true)
        {
            if (sent)
                _sentFusePermissions.UnionWith(fusePermissions);
            else
                _sentFusePermissions.ExceptWith(fusePermissions);
            return this;
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
            PersistantVariableHabbo variable = new PersistantVariableHabbo
                                                   {
                                                       habbo_id = _id,
                                                       variable_name = name
                                                   };

            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                variable = db.CreateCriteria<PersistantVariableHabbo>()
                    .Add(Example.Create(variable))
                    .UniqueResult<PersistantVariableHabbo>();
            }

            if (variable == null)
                return null;
            return variable.variable_value;
        }

        public IPersistantVariables SetPersistantVariable(string name, string value)
        {
            PersistantVariableHabbo variable = new PersistantVariableHabbo
                                                   {
                                                       habbo_id = _id,
                                                       variable_name = name,
                                                       variable_value = value
                                                   };

            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                db.SaveOrUpdate(variable);
            }
            return this;
        }

        public bool IsStalkable()
        {
            throw new NotImplementedException();
        }

        public bool IsRequestable()
        {
            throw new NotImplementedException();
        }

        public bool IsInviteable()
        {
            throw new NotImplementedException();
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
}