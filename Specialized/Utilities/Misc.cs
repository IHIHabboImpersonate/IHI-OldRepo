using System;

namespace IHI.Server.Extras
{
    public static class UsefulStuff
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Very fast random number
        /// </summary>
        public static int FastRandomNumber()
        {
            return 4; //chosen by fair dice roll.
            //guaranteed to be random.
        }

        public static byte CalculateDirection(byte X1, byte Y1, byte X2, byte Y2)
        {
            if (Y2 > Y1)
            {
                if (X2 == X1)
                    return 4;
                if (X2 < X1)
                    return 5;
                return 3;
            }
            if (Y2 < Y1)
            {
                if (X2 == X1)
                    return 0;
                if (X2 < X1)
                    return 7;
                return 1;
            }
            if (X2 < X1)
                return 6;
            return 2;
        }

        /// <summary>
        /// Might as well make it.
        /// </summary>
        public static byte MoonWalkCalculateDirection(byte X1, byte Y1, byte X2, byte Y2)
        {
            if (Y1 < Y2)
            {
                if (X1 == X2)
                    return 0;
                if (X1 > X2)
                    return 1;
                return 7;
            }
            if (Y1 > Y2)
            {
                if (X1 == X2)
                    return 4;
                if (X1 > X2)
                    return 3;
                return 5;
            }
            if (X1 > X2)
                return 2;
            return 6;
        }

        public static int GetUnixTimpstamp()
        {
            return (int) DateTime.Now.Subtract(Epoch).TotalSeconds;
        }
    }
}