using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Rooms
{
    public abstract class Human : RoomUnit
    {

        /// <summary>
        /// Make the User wave for the default length of time.
        /// </summary>
        /// <returns>The current Human object. This allows chaining.</returns>
        /// <remarks>Unsure on the naming as of yet</remarks>
        public RoomUnit SetWave(bool Active)
        {

            return this;
        }

        /// <summary>
        /// Make the User dance.
        /// </summary>
        /// <param name="Style">The style of dance to use. 0 = Stop Dancing</param>
        /// <returns>The current Human object. This allows chaining.</returns>
        public RoomUnit SetDance(byte Style)
        {

            return this;
        }
    }
}
