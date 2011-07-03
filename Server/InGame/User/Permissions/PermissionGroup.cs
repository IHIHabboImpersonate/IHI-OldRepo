using System.Collections.Generic;
using System.Linq;

namespace IHI.Server.Users.Permissions
{
    public class PermissionGroup
    {
        /// <summary>
        /// The ID of this PermissionGroup.
        /// </summary>
        private ushort fID;
        /// <summary>
        /// The name of this PermissionGroup.
        /// This is for human readability, not for identification. 
        /// </summary>
        private string fName;
        /// <summary>
        /// The actual permissions.
        /// These are used for the permission checking.
        /// </summary>
        private ushort[] fPermissions;
        /// <summary>
        /// The groups that this group include.
        /// </summary>
        private PermissionGroup[] fChildren;
        /// <summary>
        /// The groups that include this group.
        /// </summary>
        private PermissionGroup[] fParents;

        /// <summary>
        /// Constructs an empty PermissionGroup.
        /// </summary>
        /// <param name="ID">The ID of the PermissionGroup.</param>
        /// <param name="Name">The human readable name of the PermissionGroup.</param>
        internal PermissionGroup(ushort ID, string Name)
        {
            this.fID = ID;
            this.fName = Name;
        }

        /// <summary>
        /// Returns the ID of this PermissionGroup.
        /// </summary>
        public ushort GetID()
        {
            return this.fID;
        }
        /// <summary>
        /// Returns the human readable name of this PermissionGroup.
        /// </summary>
        public string GetName()
        {
            return this.fName;
        }

        /// <summary>
        /// Returns the groups that this group includes including their childen.
        /// This method is recursive.
        /// </summary>
        public PermissionGroup[] GetRecursiveChildren()
        {
            HashSet<PermissionGroup> IncludedGroups = new HashSet<PermissionGroup>(this.fChildren); // Append the children of this group.

            foreach (PermissionGroup Group in this.fChildren)
            {
                Group.GetRecursiveChildren(IncludedGroups);   // Recursivly all child groups to the HashSet.
            }

            return IncludedGroups.ToArray(); // Return the HashSet as an array.
        }
        /// <summary>
        /// Appends child groups to the given HashSet and repeats for all child groups.
        /// </summary>
        private void GetRecursiveChildren(HashSet<PermissionGroup> IncludedGroups)
        {
            IncludedGroups.UnionWith(this.fChildren);

            foreach (PermissionGroup Group in this.fChildren)
            {
                Group.GetRecursiveChildren(IncludedGroups);
            }
        }

        /// <summary>
        /// Returns the groups that this group directly includes.
        /// This method is NOT recursive.
        /// </summary>
        public PermissionGroup[] GetDirectChildren()
        {
            return this.fChildren;
        }

        /// <summary>
        /// Returns a ushort array containing the permissions in this group.
        /// </summary>
        /// <returns></returns>
        public ushort[] GetPermissions()
        {
            return this.fPermissions;
        }

        /// <summary>
        /// Adds a child to the fChildren array.
        /// </summary>
        /// <returns>The PermissionGroup that the child was added to. This is for chaining.</returns>
        internal PermissionGroup AddChild(PermissionGroup Child)
        {
            PermissionGroup[] NewArray = new PermissionGroup[this.fChildren.Length + 1];    // Create a new array one bigger than the current.

            for(int i = 0; i < this.fChildren.Length; i++)  // For every existing child...
            {
                if (this.fChildren[i] == Child)           // If the new child matches it...
                    return this;                                // Abort!

                NewArray[i] = this.fChildren[i];            // If not then add it to NewArray.
            }
            NewArray[this.fChildren.Length] = Child;      // Add the new child to the end of the NewArray (in the extra cell that was not in the old array).

            this.fChildren = NewArray;      // Replace the old fChildren array with NewArray.
            return this;
        }

        /// <summary>
        /// Adds a parent to the fParents array.
        /// </summary>
        /// <returns>The PermissionGroup that the parent was added to. This is for chaining.</returns>
        internal PermissionGroup AddParent(PermissionGroup Parent)
        {
            PermissionGroup[] NewArray = new PermissionGroup[this.fParents.Length + 1];    // Create a new array one bigger than the current.

            for (int i = 0; i < this.fParents.Length; i++)  // For every existing parent...
            {
                if (this.fParents[i] == Parent)           // If the new parent matches it...
                    return this;                                // Abort!

                NewArray[i] = this.fParents[i];            // If not then add it to NewArray.
            }
            NewArray[this.fParents.Length] = Parent;      // Add the new parent to the end of the NewArray (in the extra cell that was not in the old array).

            this.fParents = NewArray;      // Replace the old fParents array with NewArray.
            return this;
        }

        /// <summary>
        /// Adds a permission to the fPermissions array.
        /// </summary>
        /// <returns>The PermissionGroup that the parent was added to. This is for chaining.</returns>
        internal PermissionGroup AddPermission(ushort RightID)
        {
            ushort[] NewArray = new ushort[this.fPermissions.Length + 1];    // Create a new array one bigger than the current.

            for (int i = 0; i < this.fPermissions.Length; i++)  // For every existing permission...
            {
                if (this.fPermissions[i] == RightID)           // If the new permission matches it...
                    return this;                                // Abort!

                NewArray[i] = this.fPermissions[i];            // If not then add it to NewArray.
            }
            NewArray[this.fPermissions.Length] = RightID;      // Add the new permission to the end of the NewArray (in the extra cell that was not in the old array).

            this.fPermissions = NewArray;      // Replace the old fPermissions array with NewArray.
            return this;
        }
    }
}
