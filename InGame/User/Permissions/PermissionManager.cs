using System.Collections.Generic;
using System.Linq;
using IHI.Database;

namespace IHI.Server.Users.Permissions
{
    public class PermissionManager
    {
        private readonly PermissionBranch[] _permissionTreeCache; // Full tree cache
        private Dictionary<string, int> _permissionNameCache; // Name to ID Permission cache.
        private Dictionary<int, PermissionBranch> _permissionParentCache; // Cache of parentIDs

        internal PermissionManager()
        {
            #region Get the data

            IList<Permission> permissionCache; // Raw permission cache

            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                permissionCache = db.CreateCriteria<Permission>()
                    .List<Permission>();

                _permissionTreeCache = db.CreateCriteria<PermissionBranch>()
                    .List<PermissionBranch>()
                    .ToArray();
            }


            // Process the Raw Cache into the standard cache.
            foreach (var permission in permissionCache)
            {
                _permissionNameCache.Add(permission.permission_name, permission.permission_id);
            }
            foreach (var branch in _permissionTreeCache)
            {
                _permissionParentCache.Add(branch.branch_id, GenerateParentBranch(branch));
                // TODO: Test if the parent is the same instance or a copy.
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        // TODO: Test
        private PermissionBranch GenerateParentBranch(PermissionBranch branch)
        {
            return (from bdata in _permissionTreeCache
                    where bdata.branch_left < branch.branch_left
                    where bdata.branch_right > branch.branch_right
                    orderby bdata.branch_left descending
                    select bdata).First();
        }

        private PermissionBranch GetParentBranch(int branchID)
        {
            return _permissionParentCache[branchID];
        }

        private PermissionBranch GetParentBranch(PermissionBranch branch)
        {
            return GetParentBranch(branch.branch_id);
        }

        public IEnumerable<int> GetHabboPermissions(int habboID)
        {
            return (from bdata in (from hdata in _permissionTreeCache
                                   where hdata.type == (byte) PermissionBranchType.Habbo
                                   where hdata.value_id == habboID
                                   select GetParentBranch(hdata.branch_id))
                    from pdata in _permissionTreeCache
                    where pdata.type == (byte) PermissionBranchType.Permission
                    where pdata.branch_left > bdata.branch_left
                    where pdata.branch_right < bdata.branch_right
                    select pdata.value_id).ToArray();
        }
    }

    public enum PermissionBranchType
    {
        Permission = 1,
        Group = 2,
        Habbo = 3
    }
}