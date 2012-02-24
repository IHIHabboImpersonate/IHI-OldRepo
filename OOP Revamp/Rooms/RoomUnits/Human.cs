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

#endregion

namespace IHI.Server.Rooms
{
    public abstract class Human : RoomUnit
    {
        /// <summary>
        ///   Makes the Human wave.
        /// </summary>
        /// <returns>The current Human object. This allows chaining.</returns>
        /// <remarks>
        ///   Unsure on the naming as of yet
        /// </remarks>
        public RoomUnit SetWave(bool active)
        {
            throw new NotImplementedException();
            return this;
        }

        /// <summary>
        ///   Make the Human dance a given style.
        /// </summary>
        /// <param name = "style">The style of dance to use. 0 = Stop Dancing</param>
        /// <returns>The current Human object. This allows chaining.</returns>
        public RoomUnit SetDance(byte style)
        {
            // TODO: Enum
            throw new NotImplementedException();
            return this;
        }
    }
}