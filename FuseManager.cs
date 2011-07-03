using System.Collections.Generic;
using System.Data;

using Ion.Storage;

namespace IHI.Server
{
    /// <summary>
    /// Manages permissions of user accounts.
    /// </summary>
    public class FuseManager
    {
        #region Fields
        private Dictionary<ushort, FuseGroup> mGroups = new Dictionary<ushort, FuseGroup>();
        #endregion

        #region Constructors
        public FuseManager()
        {
            Core.GetStandardOut().PrintNotice("Caching fuse tables");
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                DataTable dTable = dbClient.ReadDataTable("SELECT id,name FROM fuse_group");

                for (int i = 0; i < dTable.Rows.Count; i++)
                {
                    HashSet<ushort> GroupIDs = new HashSet<ushort>();
                    mGroups.Add((ushort)dTable.Rows[i]["id"], new FuseGroup((ushort)dTable.Rows[i]["id"], (string)dTable.Rows[i]["name"], dbClient));
                }
            }
            Core.GetStandardOut().PrintNotice("Fuse tables cached");
        }
        #endregion

        #region Methods
        /// <returns>The a dictionary of all fuse groups.</returns>
        public Dictionary<ushort, FuseGroup> GetGroups()
        {
            return mGroups;
        }

        ///// <summary>
        ///// Finds all groups that the a given group contains of including all groups those groups contain, etc...
        ///// This given group is included.
        ///// </summary>
        ///// <param name="ID">The ID of the group to get the groups of.</param>
        ///// <param name="GroupIDs">This will be filled with the group IDs that the given group contains.</param>
        ///// <param name="dbClient">The DatabaseClient to use for this.</param>
        //private void RecursiveGetGroup(ushort ID, out HashSet<ushort> GroupIDs, DatabaseClient dbClient)
        //{
        //    GroupIDs.Add(ID); // Add the given group to start with.

        //    dbClient.ClearParams(); // Remove any prevois parameters for the database.
        //    dbClient.AddParamWithValue("id", ID); // Set the new parameter
        //    DataColumn dCol = dbClient.ReadDataColumn("SELECT valueid FROM fuse_item WHERE type = 'group' AND id = @id"); // Get all child group IDs

        //    if (dCol != null) // Is their are any more?
        //        for (int i = 0; i < dCol.Table.Rows.Count; i++) // Yes, loop through them all.
        //            if (!GroupIDs.Contains((ushort)dCol.Table.Rows[i]["id"])) // Is the next group already added?
        //                RecursiveGetGroup((ushort)dCol.Table.Rows[i]["id"], out GroupIDs, dbClient); // No, add it and all it's children.
        //}

        /// <summary>
        /// Get all fuserights that a user has.
        /// </summary>
        /// <param name="ID">The user ID of the user</param>
        /// <returns>A string array containing all fuse rights of the user.</returns>
        public string[] GetUserRights(uint ID)
        {
            DataColumn dCol;
            using (DatabaseClient dbClient = Core.GetDatabaseManager().GetClient())
            {
                dbClient.AddParamWithValue("id", ID); // User groups that have been specifically given to the user.
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
                Groups.UnionWith(this.mGroups[(ushort)dCol.Table.Rows[i]["groupid"]].GetChildren()); // And then add all child groups too.
            }

            foreach (ushort Group in Groups) // For every group in the collection...
            {
                Return.UnionWith(this.mGroups[Group].GetRights()); // Add the rights from it to the collection of rights the user has.
            }

            string[] ReturnArray = new string[Return.Count];
            Return.CopyTo(ReturnArray); // Convert the collection to a string array
            return ReturnArray; // Return the array.
        }
        #endregion

        internal FuseGroup GetGroup(ushort p)
        {
            return this.mGroups[p];
        }
    }

    /// <summary>
    /// Holds the details of a fuse group (user group) (Name, Rights and ChildGroups).
    /// </summary>
    public class FuseGroup
    {
        #region Fields
        private string mName; // The name of the group
        private HashSet<ushort> mChildGroups = new HashSet<ushort>(); // A collection of child groups (stored by numerical ID).
        private HashSet<string> mRights = new HashSet<string>(); // A collection of fuse rights in this group.
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of FuseGroup and loads the group infomation from the database.
        /// </summary>
        /// <param name="ID">The numerical ID of the group.</param>
        /// <param name="Name">The name of the group.</param>
        /// <param name="dbClient">The database connection to use to.</param>
        public FuseGroup(ushort ID, string Name, DatabaseClient dbClient)
        {
            this.mName = Name;
            this.mChildGroups = new HashSet<ushort>(); ;


            dbClient.ClearParams(); // Remove any previous parameters from the database connection.
            dbClient.AddParamWithValue("id", ID); // Add ID as a parameter


            DataColumn dCol = dbClient.ReadDataColumn("SELECT valueid FROM fuse_item WHERE type = 'group' AND groupid = @id"); // Get all child groups of this group.
            if (dCol != null) // If any exist...
                for (int i = 0; i < dCol.Table.Rows.Count; i++) // Loop through them all...
                    this.mChildGroups.Add((ushort)(uint)dCol.Table.Rows[i]["valueid"]); // And add each one to the collection of child groups.

            dCol = dbClient.ReadDataColumn("SELECT `fuse_right`.`right` AS `right` FROM `fuse_right`,`fuse_item` WHERE `fuse_item`.`type` = 'right' AND `fuse_item`.`groupid` = @id AND `fuse_item`.`valueid` = `fuse_right`.`id`"); // Get all fuse rights in this group from the database.

            if (dCol != null) // If there is fuse rights in this group...
                for (int i = 0; i < dCol.Table.Rows.Count; i++) // Loop through them all...
                    mRights.Add((string)dCol.Table.Rows[i]["right"]); // And add each one to the collection of fuse rights
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get the collection of fuse rights contained directly in this group.
        /// </summary>
        /// <returns>A string HashSet containing fuse rights in this group</returns>
        public HashSet<string> GetRights()
        {
            return this.mRights;
        }

        /// <summary>
        /// Recursively get the children of the group.
        /// </summary>
        /// <returns>A ushort HashSet containing the numerical IDs of the child groups.</returns>
        public HashSet<ushort> GetChildren()
        {
            HashSet<ushort> Children = this.mChildGroups; // Create a collection of and put the direct child group's numerical IDs in it.

            foreach (ushort Child in mChildGroups) // For each of those child groups...
            {
                Children.UnionWith(Core.GetFuseManager().GetGroup(Child).GetChildren()); // Add their children in it too.
            }

            return Children; // Return the collection of groups.
        }
        #endregion
    }
}