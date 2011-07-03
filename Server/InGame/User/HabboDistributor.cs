using System;
using System.Linq;
using System.Collections.Generic;
using IHI.Server.Habbos;
using IHI.Server.Networking;
using NHibernate;

namespace IHI.Server
{
    public class HabboDistributor
    {
        #region Fields
        /// <summary>
        /// The date and time of the last clean up.
        /// </summary>
        private DateTime fLastCleanUp = DateTime.Now;

        /// <summary>
        /// Stores the cached Users.
        /// </summary>
        private Dictionary<int, WeakReference> fUserCache = new Dictionary<int, WeakReference>();
        /// <summary>
        /// Stores the cached Username ID details.
        /// </summary>
        private Dictionary<string, int> fUsernameIDCache = new Dictionary<string,int>();
        // TODO: Check if it is worth adding an ID-Username cache.
        #endregion

        #region Methods
        #region Exposed Methods
        /// <summary>
        /// Return a user from the cache if possible.
        /// If the user is not cached then put it in the cache and return it.
        /// </summary>
        /// <param name="Username">The username of the user to return.</param>
        public Habbo GetUser(string Username)
        {
            lock (this)
            {
                // Is the ID cached for this username (and therefore the User)?
                if (fUsernameIDCache.ContainsKey(Username))
                {
                    // Yes, get the cached User.
                    Habbo Cached = fUserCache[fUsernameIDCache[Username]].Target as Habbo;

                    // Has the cached User been collected and removed from memory?
                    if (Cached != null)
                        // No, return the cached copy.
                        return Cached;


                    // The cached User has been collected and is no longer in memory...

                    // Remove the WeakReference to the cached User.
                    fUserCache.Remove(fUsernameIDCache[Username]);
                    // Remove the cached Username and ID.
                    fUsernameIDCache.Remove(Username);
                }

                // Load the User into memory from the database.
                Habbo TheUser = new Habbo(Username);

                // Is this a valid User?
                if (TheUser == null)
                {
                    // No, don't cache it.
                    return null;
                }

                // Yes, cache it.
                CacheUser(TheUser);

                // Return the newly cached User.
                return TheUser;
            }
        }
        /// <summary>
        /// Return a user from the cache if possible.
        /// If the user is not cached then put it in the cache and return it.
        /// </summary>
        /// <param name="ID">The ID of the user to return.</param>
        public Habbo GetUser(int ID)
        {
            lock (this)
            {
                // Is this User already cached?
                if (fUserCache.ContainsKey(ID))
                {
                    // Yes, get the cached User.
                    Habbo Cached = fUserCache[ID].Target as Habbo;

                    // Has the cached User been collected and removed from memory?
                    if (Cached != null)
                        // No, return the cached copy.
                        return Cached;

                    // Yes, we may as well do a full clean up here. We'll have to loop over it all anyway.
                    CleanUp(true);
                }

                // Load the User into memory from the database.
                Habbo TheUser = new Habbo(ID);

                // Is this a valid User?
                if (TheUser == null)
                {
                    // No, don't cache it.
                    return null;
                }

                // Yes, cache it.
                CacheUser(TheUser);

                // Return the newly cached User.
                return TheUser;
            }
        }
        /// <summary>
        /// Returns a user with a matching SSO Ticket and Origin IP.
        /// If no match is made, null is returned.
        /// </summary>
        /// <param name="SSOTicket">The SSO Ticket to match.</param>
        /// <param name="Origin">The IP Address to match.</param>
        public Habbo GetUser(string SSOTicket, int Origin)
        {
            int ID;

            using (ISession DB = Core.GetDatabaseSession())
            {
                ID =    DB.CreateCriteria<Database.Habbo>()
                            .SetProjection(NHibernate.Criterion.Projections.Property("habbo_id"))
                            .Add(new NHibernate.Criterion.EqPropertyExpression("sso_ticket", SSOTicket))
                            .Add(new NHibernate.Criterion.EqPropertyExpression("origin_ip", Origin.ToString()))
                            .List<int>().First();   // TODO: Test incorrect SSO Ticket
            }

            if (ID == -1)
                return null;

            return GetUser(ID);
        }
        /// <summary>
        /// Creates a minimal User object.
        /// This is not cached and is only used after the user connects but before logging in.
        /// Do not use this user for custom features. Use a cached version.
        /// </summary>
        /// <param name="Connection">The Connection this user is for.</param>
        /// <returns>A mostly non-function user.</returns>
        public Habbo GetPreLoginUser(IonTcpConnection Connection)
        {
            return new Habbo(Connection);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Add a User into the cache.
        /// </summary>
        /// <param name="TheUser">The user to cache.</param>
        private void CacheUser(Habbo TheUser)
        {
            lock (this)
            {
                // Cache the User.
                this.fUserCache.Add(TheUser.GetID(), new WeakReference(TheUser));
                // Cache the Username-ID.
                this.fUsernameIDCache.Add(TheUser.GetUsername(), TheUser.GetID());
            }
        }

        /// <summary>
        /// Remove any collected Users from the cache.
        /// </summary>
        /// <param name="Force">If true then ignore the time since the last clean up.</param>
        private void CleanUp(bool Force)
        {
            lock (this)
            {
                // Is force false and has it been less than 10 minutes since the last cleanup?
                if (!Force && DateTime.Now.Subtract(this.fLastCleanUp).TotalMinutes < 10)
                    // Yes, don't bother cleaning up again.
                    return;

                // Loop through all Username ID pairs in the cache 
                foreach (KeyValuePair<string, int> Ref in fUsernameIDCache)
                {
                    // Check if the User is still in memory
                    if (!fUserCache[Ref.Value].IsAlive)
                    {
                        // If it isn't remove it from the cache
                        fUserCache.Remove(Ref.Value);
                        // And remove the Username ID pairs too
                        fUsernameIDCache.Remove(Ref.Key);
                    }
                }

                // Update the last cleanup time
                this.fLastCleanUp = DateTime.Now;
            }
        }
        #endregion
        #endregion
    }
}