using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Users.Permissions
{
    class PermissionBranch
    {
        private int fID;
        private PermissionBranchType fType;
        private int fValueID;
        private int fLeft;
        private int fRight;
        private PermissionBranch fParent;

        public PermissionBranch(permission_tree RawCache)
        {
            // TODO: Complete member initialization
            this.fID = RawCache.branch_id;
            this.fType = PermissionBranchType.Branch; // TODO: Convert from string RawCache.type;
            this.fValueID = RawCache.value_id;
            this.fLeft = RawCache.branch_left;
            this.fRight = RawCache.branch_right;

            for (int i = this.fLeft; i >= 0; i++)
            {
                if
            }
        }

        /// <summary>
        /// Returns the parent PermissionBranch of this branch.
        /// </summary>
        public PermissionBranch GetParent()
        {
            return this.fParent;
        }

        /// <summary>
        /// Returns a PermissionBranch array holding all the children of this branch.
        /// </summary>
        public PermissionBranch[] GetChildren()
        {
            return Core.GetPermissionManager().GetRange(this.fLeft, this.fRight);
        }

        internal int GetLeft()
        {
            return this.fLeft;
        }
        internal int GetRight()
        {
            return this.fRight;
        }

        /// <summary>
        /// Returns true if the branch has children, false otherwise.
        /// </summary>
        public bool HasChildren()
        {
            return this.fLeft != this.fRight + 1;
        }

        /// <summary>
        /// Returns the value of this branch.
        /// </summary>
        /// <remarks>If the value type is branch then the current branch is returned unchanged.</remarks>
        /// <returns></returns>
        public object GetValue()
        {
            switch (this.fType)
            {
                case PermissionBranchType.Branch:
                    return this;
                case PermissionBranchType.Permission:
                    return Core.GetPermissionManager().GetPermission(this.fValueID);
                case PermissionBranchType.Habbo:
                    return Core.GetUserDistributor().GetUser(this.fValueID);
            }
        }
    }

    public enum PermissionBranchType
    {
        Branch,
        Permission,
        Habbo
    }
}
