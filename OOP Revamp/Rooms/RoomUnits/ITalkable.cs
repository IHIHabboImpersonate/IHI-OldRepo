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
namespace IHI.Server.Rooms
{
    public interface ITalkable
    {
        /// <summary>
        ///   Shout a message from the ITalkable to all RoomUnits in the room.
        /// </summary>
        /// <param name = "message">The message to send.</param>
        /// <returns>The current ITalkable object. This allows chaining.</returns>
        ITalkable Shout(string message);

        /// <summary>
        ///   Say a message from the ITalkable to near RoomUnits in the room.
        /// </summary>
        /// <param name = "message">The message to send.</param>
        /// <returns>The current ITalkable object. This allows chaining.</returns>
        ITalkable Say(string message);

        /// <summary>
        ///   Whisper a message from the ITalkable to another RoomUnit in the room.
        /// </summary>
        /// <param name = "recipient">The RoomUnit to recieve the message.</param>
        /// <param name = "message">The message to send.</param>
        /// <returns>The current ITalkable object. This allows chaining.</returns>
        ITalkable Whisper(ITalkable recipient, string message);
    }
}