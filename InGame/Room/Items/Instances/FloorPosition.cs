namespace IHI.Server
{
    /// <summary>
    /// Represents the a position and rotation on the floor of a room.
    /// </summary>
    public struct FloorPosition
    {
        private byte fRotation;
        private byte fX;
        private byte fY;
        private float fZ;

        /// <summary>
        /// Returns the X Coordinate (BottomLeft-TopRight).
        /// </summary>
        public byte GetX()
        {
            return fX;
        }

        /// <summary>
        /// Returns the Y Coordinate (TopLeft-BottomRight).
        /// </summary>
        public byte GetY()
        {
            return fY;
        }

        /// <summary>
        /// Returns the Z Coordinate (Top-Bottom).
        /// </summary>
        public float GetZ()
        {
            return fZ;
        }

        /// <summary>
        /// Returns the rotation (0-7).
        /// </summary>
        public byte GetRotation()
        {
            return fRotation;
        }

        /// <summary>
        /// Returns the X and Y Coordinates as a byte array.
        /// </summary>
        public byte[] Get2D()
        {
            return new[] {fX, fY};
        }

        /// <summary>
        /// Returns the X, Y and Z Coordinates as a byte array.
        /// </summary>
        public byte[] GetRounded3D()
        {
            return new[] {fX, fY, (byte) fZ};
        }

        /// <summary>
        /// Returns the X, Y and Z Coordinates as a float array.
        /// </summary>
        public float[] GetFloat3D()
        {
            return new[] {fX, fY, fZ};
        }
    }
}