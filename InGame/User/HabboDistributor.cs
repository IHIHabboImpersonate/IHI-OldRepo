using System;
using System.Collections.Generic;
using System.Linq;
using IHI.Server.Networking;
using NHibernate.Criterion;

namespace IHI.Server.Habbos
{
    public class HabboDistributor
    {
        public event HabboEventHandler OnHabboLogin;
        public event HabboEventHandler OnPreHabboLogin;

        #region Fields

        /// <summary>
        /// Stores the cached Habbos.
        /// </summary>
        private readonly Dictionary<int, WeakReference> _habboCache = new Dictionary<int, WeakReference>();

        /// <summary>
        /// Stores the cached Username ID details.
        /// </summary>
        private readonly Dictionary<string, int> _usernameIDCache = new Dictionary<string, int>();

        /// <summary>
        /// The date and time of the last clean up.
        /// </summary>
        private DateTime _lastCleanUp = DateTime.Now;

        // TODO: Check if it is worth adding an ID-Username cache.

        #endregion

        #region Methods

        #region Exposed Methods

        /// <summary>
        /// Return a Habbo from the cache if possible.
        /// If the Habbo is not cached then put it in the cache and return it.
        /// </summary>
        /// <param name="username">The username of the Habbo to return.</param>
        public Habbo GetHabbo(string username)
        {
            lock (this)
            {
                // Is the ID cached for this username (and therefore the Habbo)?
                if (_usernameIDCache.ContainsKey(username))
                {
                    // Yes, get the cached Habbo.
                    var cached = _habboCache[_usernameIDCache[username]].Target as Habbo;

                    // Has the cached Habbo been collected and removed from memory?
                    if (cached != null)
                        // No, return the cached copy.
                        return cached;


                    // The cached Habbo has been collected and is no longer in memory...

                    // Remove the WeakReference to the cached Habbo.
                    _habboCache.Remove(_usernameIDCache[username]);
                    // Remove the cached Username and ID.
                    _usernameIDCache.Remove(username);
                }

                // Load the Habbo into memory from the database.
                var theHabbo = new Habbo(username);
                
                // Yes, cache it.
                CacheHabbo(theHabbo);

                // Return the newly cached Habbo.
                return theHabbo;
            }
        }

        /// <summary>
        /// Return a Habbo from the cache if possible.
        /// If the Habbo is not cached then put it in the cache and return it.
        /// </summary>
        /// <param name="id">The ID of the Habbo to return.</param>
        public Habbo GetHabbo(int id)
        {
            lock (this)
            {
                // Is this Habbo already cached?
                if (_habboCache.ContainsKey(id))
                {
                    // Yes, get the cached Habbo.
                    var cached = _habboCache[id].Target as Habbo;

                    // Has the cached Habbo been collected and removed from memory?
                    if (cached != null)
                        // No, return the cached copy.
                        return cached;

                    // Yes, we may as well do a full clean up here. We'll have to loop over it all anyway.
                    CleanUp(true);
                }

                // Load the Habbo into memory from the database.
                var theHabbo = new Habbo(id);


                // Yes, cache it.
                CacheHabbo(theHabbo);

                // Return the newly cached Habbo.
                return theHabbo;
            }
        }

        /// <summary>
        /// Returns a Habbo with a matching SSO Ticket and Origin IP.
        /// If no match is made, null is returned.
        /// </summary>
        /// <param name="ssoTicket">The SSO Ticket to match.</param>
        /// <param name="origin">The IP Address to match.</param>
        public Habbo GetHabbo(string ssoTicket, int origin)
        {
            int id;

            using (var db = CoreManager.ServerCore.GetDatabaseSession())
            {
                id = db.CreateCriteria<Database.Habbo>()
                    .SetProjection(Projections.Property("habbo_id"))
                    .Add(Restrictions.Eq("sso_ticket", ssoTicket))
                    .Add(Restrictions.Eq("origin_ip", origin))
                    .List<int>().FirstOrDefault();
            }

            if (id == 0)
                return null;

            return GetHabbo(id);
        }

        /// <summary>
        /// Return a Habbo from the cache if possible.
        /// If the Habbo is not cached then put it in the cache and return it.
        /// FYI: If the Habbo is not cached then this the Habbo will be created from the data provided.
        /// </summary>
        /// <param name="habbo">The database result of the Habbo to return.</param>
        public Habbo GetHabbo(Database.Habbo habbo)
        {
            lock (this)
            {
                // Is this Habbo already cached?
                if (_habboCache.ContainsKey(habbo.habbo_id))
                {
                    // Yes, get the cached Habbo.
                    var cached = _habboCache[habbo.habbo_id].Target as Habbo;

                    // Has the cached Habbo been collected and removed from memory?
                    if (cached != null)
                        // No, return the cached copy.
                        return cached;

                    // Yes, we may as well do a full clean up here. We'll have to loop over it all anyway.
                    CleanUp(true);
                }

                // Load the Habbo into memory from the database.
                var theHabbo = new Habbo(habbo);

                // Yes, cache it.
                CacheHabbo(theHabbo);

                // Return the newly cached Habbo.
                return theHabbo;
            }
        }

        /// <summary>
        /// Creates a minimal Habbo object.
        /// This is not cached and is only used after the Habbo connects but before logging in.
        /// Do not use this Habbo for custom features. Use a cached version.
        /// </summary>
        /// <param name="connection">The Connection this Habbo is for.</param>
        /// <returns>A mostly non-function Habbo.</returns>
        public Habbo GetPreLoginHabbo(IonTcpConnection connection)
        {
            return new Habbo(connection);
        }

        #endregion

        internal void InvokeOnHabboLogin(object source, HabboEventArgs e)
        {
            OnHabboLogin.Invoke(source, e);
        }
        internal void InvokeOnPreHabboLogin(object source, HabboEventArgs e)
        {
            OnHabboLogin.Invoke(source, e);
        }

        #region Private Methods

        /// <summary>
        /// Add a Habbo into the cache.
        /// </summary>
        /// <param name="theHabbo">The Habbo to cache.</param>
        private void CacheHabbo(Habbo theHabbo)
        {
            lock (this)
            {
                // Cache the Habbo.
                _habboCache.Add(theHabbo.GetID(), new WeakReference(theHabbo));
                // Cache the Username-ID.
                _usernameIDCache.Add(theHabbo.GetUsername(), theHabbo.GetID());
            }
        }

        /// <summary>
        /// Remove any collected Habbos from the cache.
        /// </summary>
        /// <param name="force">If true then ignore the time since the last clean up.</param>
        private void CleanUp(bool force)
        {
            lock (this)
            {
                // Is force false and has it been less than 10 minutes since the last cleanup?
                if (!force && DateTime.Now.Subtract(_lastCleanUp).TotalMinutes < 10)
                    // Yes, don't bother cleaning up again.
                    return;

                var toRemoveUsernames = new List<string>();

                // Loop through all Username ID pairs in the cache 
                foreach (var weakref in _usernameIDCache.Where(weakref => !_habboCache[weakref.Value].IsAlive))
                {
                    // If it isn't remove it from the cache
                    _habboCache.Remove(weakref.Value);
                    // And remove the Username ID pairs too
                    toRemoveUsernames.Add(weakref.Key);
                }
                foreach (var username in toRemoveUsernames)
                    _usernameIDCache.Remove(username);


                // Update the last cleanup time
                _lastCleanUp = DateTime.Now;
            }
        }

        #endregion

        #endregion
    }
}