using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IHI.Server.Rooms;

namespace IHI.Server.Habbos
{
    public interface IBefriendable
    {
        /// <summary>
        /// Returns the ID of the IBefriendable.
        /// </summary>
        int GetID();
        /// <summary>
        /// Returns the display name of the IBefriendable.
        /// </summary>
        string GetDisplayName();
        /// <summary>
        /// Returns the motto of the IBefriendable.
        /// </summary>
        string GetMotto();
        /// <summary>
        /// Returns the date of the IBefriendable's last access.
        /// </summary>
        DateTime GetLastAccess();
    }
}
