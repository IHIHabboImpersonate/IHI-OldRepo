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

using System.Collections.Generic;
using System.Linq;
using IHI.Database;
using NHibernate;

#endregion

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

            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                permissionCache = db.CreateCriteria<Permission>()
                    .List<Permission>();

                _permissionTreeCache = db.CreateCriteria<PermissionBranch>()
                    .List<PermissionBranch>()
                    .ToArray();
            }


            // Process the Raw Cache into the standard cache.
            foreach (Permission permission in permissionCache)
            {
                _permissionNameCache.Add(permission.permission_name, permission.permission_id);
            }
            foreach (PermissionBranch branch in _permissionTreeCache)
            {
                _permissionParentCache.Add(branch.branch_id, GenerateParentBranch(branch));
                // TODO: Test if the parent is the same instance or a copy.
            }

            #endregion
        }

        /// <summary>
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

        internal IEnumerable<int> GetHabboPermissions(int habboID)
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