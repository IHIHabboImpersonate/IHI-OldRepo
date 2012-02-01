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
using System.Collections.Generic;

#endregion

namespace IHI.Server.Rooms
{
    public abstract class RoomUnit : IRollerable, ITalkable
    {
        protected FloorPosition Destination;
        protected string DisplayName;
        protected IFigure Figure;
        protected Queue<byte[]> Path;
        protected FloorPosition Position;
        protected Room Room;

        #region IRollerable Members

        public IRollerable Roll(FloorPosition to)
        {
            throw new NotImplementedException();
        }

        public IRollerable Roll(FloorPosition from, FloorPosition to)
        {
            throw new NotImplementedException();
        }

        public FloorPosition GetPosition()
        {
            throw new NotImplementedException();
        }

        public RoomUnit SetPosition(FloorPosition position)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITalkable Members

        public ITalkable Shout(string message)
        {
            throw new NotImplementedException();
            return this;
        }

        public ITalkable Whisper(ITalkable recipient, string message)
        {
            throw new NotImplementedException();
            return this;
        }

        public ITalkable Say(string message)
        {
            throw new NotImplementedException();
            return this;
        }

        #endregion

        /// <summary>
        ///   Returns the current Room the RoomUnit is in.
        /// </summary>
        /// <returns></returns>
        public Room GetRoom()
        {
            return Room;
        }

        /// <summary>
        ///   Sets the current Room the RoomUnit is in.
        /// </summary>
        /// <returns></returns>
        public RoomUnit SetRoom(Room room)
        {
            Room = room;
            return this;
        }

        /// <summary>
        ///   Send the current details of the User to the room.
        ///   The user will poof when used.
        ///   No changes to the User object are made.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit Refresh()
        {
            return this;
        }

        /// <summary>
        ///   Sets the desired position in the room of this user.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit SetDestination(FloorPosition destination)
        {
            Destination = destination;
            return this;
        }

        /// <summary>
        ///   Returns the desired position in the room of this user.
        /// </summary>
        public FloorPosition GetDestination()
        {
            return Destination;
        }

        public IFigure GetFigure()
        {
            return Figure;
        }

        public RoomUnit SetFigure(IFigure figure)
        {
            Figure = figure;
            return this;
        }

        public string GetDisplayName()
        {
            return DisplayName;
        }

        public RoomUnit SetDisplayName(string displayName)
        {
            DisplayName = displayName;
            return this;
        }

        public byte[] GetNextStep()
        {
            if (Path.Count == 0)
                return new byte[0];

            return Path.Dequeue();
        }

        public RoomUnit AddNextStep(byte[] step)
        {
            Path.Enqueue(step);
            return this;
        }

        public RoomUnit SetPath(ICollection<byte[]> path)
        {
            Path = (Queue<byte[]>) path;
            return this;
        }

        public RoomUnit ResetPath()
        {
            Path.Clear();
            return this;
        }
    }
}