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
namespace IHI.Server
{
    /// <summary>
    ///   Represents the a position and rotation on the floor of a room.
    /// </summary>
    public struct FloorPosition
    {
        /// <summary>
        ///   The rotation (0-7).
        /// </summary>
        public byte Rotation { get; set; }

        /// <summary>
        ///   The X Coordinate (BottomLeft-TopRight)
        /// </summary>
        public byte X { get; set; }

        /// <summary>
        ///   The Y Coordinate (TopLeft-BottomRight).
        /// </summary>
        public byte Y { get; set; }

        /// <summary>
        ///   The Z Coordinate (Top-Bottom).
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        ///   Returns the X and Y Coordinates as a byte array.
        /// </summary>
        public byte[] Get2D()
        {
            return new[] {X, Y};
        }

        /// <summary>
        ///   Returns the X, Y and Z Coordinates as a byte array.
        /// </summary>
        public byte[] GetRounded3D()
        {
            return new[] {X, Y, (byte) Z};
        }

        /// <summary>
        ///   Returns the X, Y and Z Coordinates as a float array.
        /// </summary>
        public float[] GetFloat3D()
        {
            return new[] {X, Y, Z};
        }
    }
}