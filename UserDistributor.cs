using System;
using System.Collections.Generic;

using Ion.Net.Connections;
using Ion.Storage;

namespace IHI.Server
{
    public class UserDistributor
    {
        #region Fields
        /// <summary>
        /// The date and time of the last clean up.
        /// </summary>
        private DateTime fLastCleanUp = DateTime.Now;

        /// <summary>
        /// Stores the cached Users.
        /// </summary>
        private Dictionary<uint, WeakReference> fUserCache = new Dictionary<uint, WeakReference>();
        /// <summary>
        /// Stores the cached Username ID details.
        /// </summary>
        private Dictionary<string, uint> fUsernameIDCache = new Dictionary<string,uint>();
        // TODO: Check if it is worth adding an ID-Username cache.
        #endregion

        #region Methods
        #region Exposed Methods
        /// <summary>
        /// Return a user from the cache if possible.
        /// If the user is not cached then put it in the cache and return it.
        /// </summary>
        /// <param name="Username">The username of the user to return.</param>
        public User GetUser(string Username)
        {
            lock (this)
            {
                // Is the ID cached for this username (and therefore the User)?
                if (fUsernameIDCache.ContainsKey(Username))
                {
                    // Yes, get the cached User.
                    User Cached = fUserCache[fUsernameIDCache[Username]].Target as User;

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
                User TheUser = new User(Username);

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
        public User GetUser(uint ID)
        {
            lock (this)
            {
                // Is this User already cached?
                if (fUserCache.ContainsKey(ID))
                {
                    // Yes, get the cached User.
                    User Cached = fUserCache[ID].Target as User;

                    // Has the cached User been collected and removed from memory?
                    if (Cached != null)
                        // No, return the cached copy.
                        return Cached;

                    // Yes, we may aswell do a full clean up here. We'll have to loop over it all anyway.
                    CleanUp(true);
                }

                // Load the User into memory from the database.
                User TheUser = new User(ID);

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
        public User GetUser(string SSOTicket, string Origin)
        {
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("sso", SSOTicket);
                dbClient.AddParamWithValue("origin", Origin);

                int ID = dbClient.ReadInt32("SELECT id FROM users WHERE ssoticket = @sso AND originip = @origin");

                if (ID == 0)
                    return null;

                return GetUser((uint)ID);
            }
        }
        /// <summary>
        /// Creates a minimal User object.
        /// This is not cached and is only used after the user connects but before logging in.
        /// Do not use this user for custom features. Use a cached version.
        /// </summary>
        /// <param name="Connection">The connection this user is for.</param>
        /// <returns>A mostly non-function user.</returns>
        public User GetPreLoginUser(IonTcpConnection Connection)
        {
            return new User(Connection);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Add a User into the cache.
        /// </summary>
        /// <param name="TheUser">The user to cache.</param>
        private void CacheUser(User TheUser)
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
                foreach (KeyValuePair<string, uint> Ref in fUsernameIDCache)
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