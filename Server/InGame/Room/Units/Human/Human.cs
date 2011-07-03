using System;

namespace IHI.Server
{
    public abstract class Human : RoomUnit
    {
        /// <summary>
        /// The gender of the user.
        /// Male = True
        /// Female = False
        /// </summary>
        protected bool fGender;
        
        public bool GetGender()
        {
            return this.fGender;
        }
        public Human SetGender(bool Gender)
        {
            this.fGender = Gender;
            return this;
        }

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

        private uint fSwimFigure;
        /// <summary>
        /// Returns a byte array containing 3 values.
        /// The values are the RGB colour values of the swim figure.
        /// </summary>
        public byte[] GetSwimFigure()
        {
            return new byte[] { (byte)(this.fSwimFigure >> 16), (byte)(this.fSwimFigure << 8 >> 16), (byte)((this.fSwimFigure << 16) >> 16) };
        }

        /// <summary>
        /// Returns a byte array containing 3 values.
        /// The values are the RGB colour values of the swim figure.
        /// </summary>
        public string GetFormattedSwimFigure()
        {
            return "ch=s0" + (this.fGender ? '1' : '2') + "/" + (this.fSwimFigure >> 16) + "," + (this.fSwimFigure << 8 >> 16) + "," + ((this.fSwimFigure << 16) >> 16);
        }
        /// <summary>
        /// Sets the colour of the swim figure.
        /// </summary>
        /// <param name="Red">The amount of red in the colour.</param>
        /// <param name="Green">The amount of green in the colour.</param>
        /// <param name="Blue">The amount of blue in the colour.</param>
        /// <returns></returns>
        public Human SetSwimFigure(byte Red, byte Green, byte Blue)
        {
            this.fSwimFigure = (uint)((Red << 16) | Green << 8) | Blue;
            return this;
        }
    }
}
