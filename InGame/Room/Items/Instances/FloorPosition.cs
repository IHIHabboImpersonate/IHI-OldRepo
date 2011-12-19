namespace IHI.Server
{
    /// <summary>
    /// Represents the a position and rotation on the floor of a room.
    /// </summary>
    public struct FloorPosition
    {
        /// <summary>
        /// The rotation (0-7).
        /// </summary>
        public byte Rotation
        {
            get;
            set;
        }
        /// <summary>
        /// The X Coordinate (BottomLeft-TopRight)
        /// </summary>
        public byte X
        {
            get;
            set;
        }
        /// <summary>
        /// The Y Coordinate (TopLeft-BottomRight).
        /// </summary>
        public byte Y
        {
            get;
            set;
        }
        /// <summary>
        /// The Z Coordinate (Top-Bottom).
        /// </summary>
        public float Z
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the X and Y Coordinates as a byte array.
        /// </summary>
        public byte[] Get2D()
        {
            return new[] {X, Y};
        }

        /// <summary>
        /// Returns the X, Y and Z Coordinates as a byte array.
        /// </summary>
        public byte[] GetRounded3D()
        {
            return new[] {X, Y, (byte) Z};
        }

        /// <summary>
        /// Returns the X, Y and Z Coordinates as a float array.
        /// </summary>
        public float[] GetFloat3D()
        {
            return new[] {X, Y, Z};
        }
    }
}