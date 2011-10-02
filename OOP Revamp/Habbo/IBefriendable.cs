using System;
using IHI.Server.Rooms;

namespace IHI.Server.Habbos
{
    public interface IBefriendable : IInstanceVariables, IPersistantVariables
    {
        /// <summary>
        /// Returns the ID of the IBefriendable.
        /// </summary>
        int GetID();

        /// <summary>
        /// Returns true if the IBefriendable is logged in.
        /// </summary>
        bool IsLoggedIn();

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

        /// <summary>
        /// Returns the figure of the IBefriendable.
        /// </summary>
        IFigure GetFigure();

        /// <summary>
        /// Returns the room the IBefriendable is in.
        /// </summary>
        Room GetRoom();
    }
}