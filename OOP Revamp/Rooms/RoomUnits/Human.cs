using System;

namespace IHI.Server.Rooms
{
    public abstract class Human : RoomUnit
    {
        /// <summary>
        /// Makes the Human wave.
        /// </summary>
        /// <returns>The current Human object. This allows chaining.</returns>
        /// <remarks>Unsure on the naming as of yet</remarks>
        public RoomUnit SetWave(bool active)
        {
            throw new NotImplementedException();
            return this;
        }

        /// <summary>
        /// Make the Human dance a given style.
        /// </summary>
        /// <param name="style">The style of dance to use. 0 = Stop Dancing</param>
        /// <returns>The current Human object. This allows chaining.</returns>
        public RoomUnit SetDance(byte style)
        {
            throw new NotImplementedException();
            return this;
        }
    }
}