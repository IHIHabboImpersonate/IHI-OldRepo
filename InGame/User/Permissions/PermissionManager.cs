using System.Linq;
using System.Collections.Generic;
using System.Data;

using NHibernate;
using NHibernate.Linq;

using IHI.Database;

namespace IHI.Server.Users.Permissions
{
    public class PermissionManager
    {
        private Dictionary<string, int> fPermissionNameCache;                   // Name to ID Permission cache.
        private PermissionBranch[] fPermissionTreeCache;                        // Full tree cache
        private Dictionary<int, PermissionBranch> fPermissionParentCache;       // Cache of parentIDs

        internal PermissionManager()
        {
            #region Get the data
            IList<Permission> PermissionCache;                                       // Raw permission cache

            using (ISession DB = CoreManager.GetCore().GetDatabaseSession())
            {
                PermissionCache = DB.CreateCriteria<Permission>()
                                        .List<Permission>();

                this.fPermissionTreeCache = DB.CreateCriteria<PermissionBranch>()
                                                .List<PermissionBranch>()
                                                    .ToArray();
            }

                        
            // Process the Raw Cache into the standard cache.
            foreach (Permission P in PermissionCache)
            {
                this.fPermissionNameCache.Add(P.permission_name, P.permission_id);
            }
            foreach (PermissionBranch B in this.fPermissionTreeCache)
            {
                this.fPermissionParentCache.Add(B.branch_id, GenerateParentBranch(B));
                // TODO: Test if the parent is the same instance or a copy.
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        // TODO: Test
        private PermissionBranch GenerateParentBranch(PermissionBranch Branch)
        {
            return (from bdata in this.fPermissionTreeCache
                    where bdata.branch_left < Branch.branch_left
                    where bdata.branch_right > Branch.branch_right
                    orderby bdata.branch_left descending
                    select bdata).First();
        }
        private PermissionBranch GetParentBranch(int BranchID)
        {
            return this.fPermissionParentCache[BranchID];
        }
        private PermissionBranch GetParentBranch(PermissionBranch Branch)
        {
            return GetParentBranch(Branch.branch_id);
        }

        public int[] GetHabboPermissions(int HabboID)
        {
            return (from bdata in (from hdata in this.fPermissionTreeCache
                                   where hdata.type == (byte)PermissionBranchType.Habbo
                                   where hdata.value_id == HabboID
                                   select GetParentBranch(hdata.branch_id))
                    from pdata in this.fPermissionTreeCache
                    where pdata.type == (byte)PermissionBranchType.Permission
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
