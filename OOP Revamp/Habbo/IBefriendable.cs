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

using System;
using IHI.Server.Rooms;

#endregion

namespace IHI.Server.Habbos
{
    public interface IBefriendable : IInstanceVariables, IPersistantVariables
    {
        bool BlockStalking { get; set; }
        bool BlockRequests { get; set; }
        bool BlockInvites { get; set; }
        event MessengerBlockFlagEventHandler OnBlockFlagChanged;

        /// <summary>
        ///   Returns the ID of the IBefriendable.
        /// </summary>
        int GetID();

        /// <summary>
        ///   Returns true if the IBefriendable is logged in.
        /// </summary>
        bool IsLoggedIn();

        /// <summary>
        ///   Returns the display name of the IBefriendable.
        /// </summary>
        string GetDisplayName();

        /// <summary>
        ///   Returns the motto of the IBefriendable.
        /// </summary>
        string GetMotto();

        /// <summary>
        ///   Returns the date of the IBefriendable's last access.
        /// </summary>
        DateTime GetLastAccess();

        /// <summary>
        ///   Returns the figure of the IBefriendable.
        /// </summary>
        IFigure GetFigure();

        /// <summary>
        ///   Returns the room the IBefriendable is in.
        /// </summary>
        Room GetRoom();

        bool IsStalkable();
        bool IsRequestable();
        bool IsInviteable();
    }
}